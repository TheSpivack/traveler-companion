using TravelerCompanion.Models;

namespace TravelerCompanion;

/// <summary>
/// Interface for the airport data provider.
/// </summary>
public interface IAirportDataProvider
{
    /// <summary>
    /// Gets the airport info by code.
    /// </summary>
    Task<AirportInfo?> GetAirportInfoAsync(string airportCode);
}