using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using TravelerCompanion.Models;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs;

public class SouthwestWebDriverException : Exception
{
    public SouthwestWebDriverException() : base(){}
    public SouthwestWebDriverException(string message) : base(message){}
    public SouthwestWebDriverException(string message, Exception innerException) : base(message, innerException){}
    
}

public interface ISouthwestWebDriver
{
    /// <summary>
    /// Logs into the account and retrieves the reservations.
    /// </summary>
    public Task<(AccountMonitorState account, IEnumerable<ReservationMonitorState> reservations)> RetrieveAccountAndReservationsAsync(string username, string password);
    
    /// <summary>
    /// Updates the reservation and returns the current flights.
    /// </summary>
    public Task<IEnumerable<FlightBoundInformation>> RetrieveFlightBoundsAsync(ReservationMonitorState reservation);

    /// <summary>
    /// Loads the check-in page and returns the request headers needed to call southwest API and whatnot
    /// </summary>
    public Task<IDictionary<string, string>> RefreshRequestHeadersAsync();
}

public class SouthwestWebDriver(IHttpClientFactory httpClientFactory, IAirportDataProvider airportDataProvider, ILogger<SouthwestWebDriver> logger) : ISouthwestWebDriver
{
    private const string BaseUrl = "https://mobile.southwest.com";
    private const int InvalidCredentialsCode = 400518024;
    
    private static string LoginUrl => $"{BaseUrl}/api/security/v4/security/token";
    private static string CheckinUrl => $"{BaseUrl}/check-in";
    private static string CheckinHeadersUrl => $"{BaseUrl}/api/chase/v2/chase/offers";
    private static string TripsUrl => $"{BaseUrl}/api/mobile-misc/v1/mobile-misc/page/upcoming-trips";
    private static string ViewReservationUrl => $"{BaseUrl}/api/mobile-air-booking/v1/mobile-air-booking/page/view-reservation";
    
    public async Task<IDictionary<string, string>> RefreshRequestHeadersAsync()
    {
        logger.LogTrace("Creating puppeteer instance");
        var puppet = new PuppeteerExtra();
        puppet.Use(new StealthPlugin());
        logger.LogTrace("Launching browser");
        await using var browser = await puppet.LaunchAsync(new()
        {
            Headless = true
        });
        var (page, headers) = await LoadCheckInPageAsync(browser);
        if (page != null)
        {
            await page.CloseAsync();
        }
        return headers;
    }

    public async Task<(AccountMonitorState account, IEnumerable<ReservationMonitorState> reservations)> RetrieveAccountAndReservationsAsync(string username, string password)
    {
        logger.LogTrace("Creating puppeteer instance");
        var puppet = new PuppeteerExtra();
        puppet.Use(new StealthPlugin());
        logger.LogTrace("Launching browser");
        await using var browser = await puppet.LaunchAsync(new()
        {
            Headless = true
        });
        
        var (checkinPage, headers) = await LoadCheckInPageAsync(browser);
        if (checkinPage == null)
        {
            throw new SouthwestWebDriverException("Failed to load check-in page");
        }

        try
        {
            await Task.Delay(100);
            logger.LogTrace("Clicking login button");
            await checkinPage.ClickAsync(".login-button--box");
            logger.LogTrace("Filling out username");
            await checkinPage.TypeAsync("input[name=\"userNameOrAccountNumber\"]", username);
            logger.LogTrace("Filling out password");
            await checkinPage.TypeAsync("input[name=\"password\"]", password);
            logger.LogTrace("Clicking login button");
            await checkinPage.ClickAsync("button#login-btn");

            var loginResponseTask = checkinPage.WaitForResponseAsync(LoginUrl);
            var upcomingTripsTask = checkinPage.WaitForResponseAsync(TripsUrl);

            var loginResponse = await loginResponseTask;
            if (loginResponse.Status != HttpStatusCode.OK)
            {
                var errorBody = await loginResponse.TextAsync();
                logger.LogError(errorBody);
                throw new SouthwestWebDriverException($"Error response from login: {loginResponse.Status}");
            }

            var account = new AccountMonitorState
            {
                Username = username,
                Password = password,
                RequestHeaders = headers
            };
            logger.LogDebug("Successfully logged in account {Username}", account.Username);
            
            var loginResponseJson = await loginResponse.JsonAsync();
            account.FirstName = loginResponseJson["customers.userInformation.firstName"]?.ToString();
            account.LastName = loginResponseJson["customers.userInformation.lastName"]?.ToString();
            account.AccountNumber = loginResponseJson["customers.userInformation.accountNumber"]?.ToString();
            account.RedeemablePoints = loginResponseJson["customers.userInformation.redeemablePoints"]?.ToObject<int>();

            var upcomingTripsResponse = await upcomingTripsTask;
            if (upcomingTripsResponse.Status != HttpStatusCode.OK)
            {
                var errorBody = await upcomingTripsResponse.TextAsync();
                logger.LogError(errorBody);
                throw new SouthwestWebDriverException(
                    $"Error response from upcoming trips: {upcomingTripsResponse.Status}");
            }
            logger.LogDebug("Successfully retrieved upcoming trips page");

            var upcomingTripsResponseJson = await upcomingTripsResponse.JsonAsync();
            var reservations = upcomingTripsResponseJson["upcomingTripsPage"]?.Select(trip => new ReservationMonitorState
            {
                ConfirmationNumber = trip["confirmationNumber"]?.ToString() ?? "",
                FirstName = account.FirstName ??"unknown",
                LastName = account.LastName ??"unknown",
                AccountUsername = account.Username,
                RequestHeaders = headers
            }) ?? Array.Empty<ReservationMonitorState>();

            await checkinPage.CloseAsync();

            return (account, reservations);
        }
        catch (PuppeteerException ex)
        {
            var screenshotFile = @$"C:\Code\logs\PuppeteerException.{DateTime.Now:yyyy-MM-dd.HH-mm-ss}.png";
            await checkinPage.ScreenshotAsync(screenshotFile);
            logger.LogError("Exception thrown: {Message}. Screenshot saved to {ScreenshotFile}", ex.Message, screenshotFile);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the flights for the reservation via an API call.  If the reservation doesn't already have
    /// Southwest request headers set, it will load the check-in page to set them.
    /// </summary>
    /// <exception cref="SouthwestWebDriverException"></exception>
    public async Task<IEnumerable<FlightBoundInformation>> RetrieveFlightBoundsAsync(ReservationMonitorState reservation)
    {
        logger.LogDebug("Retrieving reservation information");
        if (!reservation.RequestHeaders.Any())
        {
            logger.LogDebug("No headers found.  Loading check-in page to set headers");
            logger.LogTrace("Creating puppeteer instance");
            var puppet = new PuppeteerExtra();
            puppet.Use(new StealthPlugin());
            logger.LogTrace("Launching browser");
            await using var browser = await puppet.LaunchAsync(new()
            {
                Headless = true
            });
            var (_, headers) = await LoadCheckInPageAsync(browser);
            reservation.RequestHeaders = headers;
        }
        var response = await MakeApiRequestAsync($"{ViewReservationUrl}/{reservation.ConfirmationNumber}", HttpMethod.Post,
            reservation.RequestHeaders,
            body: new Dictionary<string, string>
            {
                ["firstName"] = reservation.FirstName,
                ["lastName"] = reservation.LastName,
                ["recordLocator"] = reservation.ConfirmationNumber
            });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new SouthwestWebDriverException($"Error retrieving reservation. Status code: {response.StatusCode}\r\n{body}");
        }
        
        logger.LogDebug("Successfully retrieved reservation information");
        await using var bodyStream = await response.Content.ReadAsStreamAsync();
        var reservationInfo = await JsonNode.ParseAsync(bodyStream);
        if (reservationInfo == null)
        {
            throw new SouthwestWebDriverException("Error parsing reservation information");
        }

        var flightsBounds = new List<FlightBoundInformation>();
        foreach (var bound in reservationInfo["viewReservationViewPage"]?["bounds"]?.AsArray() ?? [])
        {
            var flightBound = await GetFlightBoundInformationAsync(bound);
            if (flightBound is not null)
            {
                flightsBounds.Add(flightBound);    
            }
        }
        return flightsBounds;
    }

    internal async Task<FlightBoundInformation?> GetFlightBoundInformationAsync(JsonNode? bound)
    {
        if (bound is null)
        {
            logger.LogWarning("Bound is null.  Whatever that means.");
            return null;
        }

        if (bound["departureStatus"]?.ToString().Equals("DEPARTED", StringComparison.OrdinalIgnoreCase) == true)
        {
            logger.LogDebug("Skipping departed bound {@Bound}", bound);
            return null;
        }

        var departCode = bound["departureAirport"]?["code"]?.ToString();
        if (string.IsNullOrWhiteSpace(departCode))
        {
            throw new SouthwestWebDriverException("Unable to locate a departure airport code for the reservation");
        }
        var departureAirport = await airportDataProvider.GetAirportInfoAsync(departCode);
        if (departureAirport == null)
        {
            throw new SouthwestWebDriverException($"Unable to locate airport information for {departCode}");
        }
            
        var arrivalCode = bound["arrivalAirport"]?["code"]?.ToString();
        if (string.IsNullOrWhiteSpace(arrivalCode))
        {
            throw new SouthwestWebDriverException("Unable to locate an arrival airport code for the reservation");
        }
        var arrivalAirport = await airportDataProvider.GetAirportInfoAsync(arrivalCode);
        if (arrivalAirport == null)
        {
            throw new SouthwestWebDriverException($"Unable to locate airport information for {arrivalCode}");
        }

        return new FlightBoundInformation
        (
            departureAirport,
            GetFlightTime(bound["departureDate"], bound["departureTime"], departureAirport),
            arrivalAirport
        );
        
        DateTimeOffset GetFlightTime(JsonNode? dateNode, JsonNode? timeNode, AirportInfo airport)
        {
            ArgumentNullException.ThrowIfNull(dateNode);
            ArgumentNullException.ThrowIfNull(timeNode);
            
            var tzString = (airport.TimeZone.BaseUtcOffset < TimeSpan.Zero ? @"\-" : "") + @"hh\:mm";
            return DateTimeOffset.ParseExact($"{dateNode} {timeNode} {airport.TimeZone.BaseUtcOffset.ToString(tzString)}", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///  Loads the check-in page, which also sets the headers of the session
    /// </summary>
    /// <exception cref="SouthwestWebDriverException"></exception>
    private async Task<(IPage? page, IDictionary<string, string> headers)> LoadCheckInPageAsync(IBrowser browser)
    {
        logger.LogTrace("Creating new page");
        var page = await browser.NewPageAsync();
        logger.LogDebug("Navigating to {CheckinUrl}", CheckinUrl);
        var waitForHeadersRequestTask = page.WaitForRequestAsync(CheckinHeadersUrl);
        await page.GoToAsync(CheckinUrl);
        logger.LogDebug("Waiting for request to {CheckinHeadersUrl} to sniff headers", CheckinHeadersUrl);
        var request = await waitForHeadersRequestTask;
        logger.LogTrace("Reading headers from request");
        var headers = request.Headers.Where(header =>
                Regex.IsMatch(header.Key, @"x-api-key|x-channel-id|user-agent|^[\w-]+?-\w$", RegexOptions.IgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value);

        if (headers.Count <= 0)
        {
            throw new SouthwestWebDriverException("No headers found");
        }
        logger.LogDebug("Headers successfully set: {@Headers}", (object) headers.Keys);
        return (page, headers);
    }
    
    private async Task<HttpResponseMessage> MakeApiRequestAsync(string url, HttpMethod method, IDictionary<string, string> requestHeaders, object? body = null, IDictionary<string, string>? queryString = null)
    {
        var urlBuilder = new UriBuilder(url);
        if(queryString != null)
        {
            urlBuilder.Query = string.Join("&", queryString.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
        
        var request = new HttpRequestMessage(method, urlBuilder.Uri);
        foreach (var (key, value) in requestHeaders)
        { 
            request.Headers.Add(key, value);
        }
        
        if(method == HttpMethod.Post && body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        var httpClient = httpClientFactory.CreateClient(nameof(SouthwestWebDriver));
        return await httpClient.SendAsync(request);
    }
}
