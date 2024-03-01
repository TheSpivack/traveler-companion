namespace TravelerCompanion.Models;


public record AirportInfo(string Code, TimeZoneInfo TimeZone, string Description);


public record FlightBoundInformation(
    AirportInfo DepartureAirport,
    DateTimeOffset DepartureTime,
    AirportInfo ArrivalAirport);
