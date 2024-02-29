using System.Reflection;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;

namespace TravelerCompanion;

/// <summary>
/// Implementation of <see cref="IAirportDataProvider"/> that loads data from the provided csv file.  Loads the file
/// into memory at startup so it's fast.
/// </summary>
public class CsvAirportDataProvider : IAirportDataProvider
{
	private readonly IDictionary<string, AirportInfo> _allAirports;

	public Task<AirportInfo?> GetAirportInfoAsync(string airportCode)
	{
		_allAirports.TryGetValue(airportCode.ToUpper(), out var airportInfo);
		return Task.FromResult(airportInfo);
	}

	public CsvAirportDataProvider(ILogger<CsvAirportDataProvider> logger, string? airportDataFilePath = null)
	{
		if (airportDataFilePath == null)
		{
			var exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (exeLocation is null)
			{
				throw new ApplicationException("Unable to locate exe directory");
			}
			airportDataFilePath = Path.Combine(exeLocation, "airportdata.csv");
		}
		
		var airportDetails = File.ReadAllLines(airportDataFilePath);
		_allAirports = new Dictionary<string, AirportInfo>();
		foreach (var airportDetail in airportDetails)
		{
			var parts = airportDetail.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
			try
			{
				var airportInfo = new AirportInfo(parts[0], TimeZoneInfo.FindSystemTimeZoneById(parts[1]), parts[2]);
				_allAirports.Add(airportInfo.Code, airportInfo);
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Error parsing info for airport info: {Code} {TimeZone} {Description}", parts[0], parts[1], parts[2]);
			}
		}
	}
}