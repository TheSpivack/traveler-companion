using TravelerCompanion.Models;

namespace TravelerCompanion.SouthwestJobs.Models;

/// <summary>
/// Base class for the check-in jobs
/// </summary>
public abstract class CheckInJobState : SingleShotJobState
{
    /// <summary>
    /// Confirmation number
    /// </summary>
    public required string ConfirmationNumber { get; init; }
    
    /// <summary>
    /// First name for the reservation
    /// </summary>
    public required string FirstName { get; init; }
    
    /// <summary>
    /// Last name for the reservation
    /// </summary>
    public required string LastName { get; init; }
    
    /// <summary>
    /// Departure airport information.
    /// </summary>
    public required AirportInfo DepartureAirport { get; init; }
    
    /// <summary>
    /// Departure time
    /// </summary>
    public required DateTimeOffset DepartureTime { get; init; }
    
    /// <summary>
    /// Departure airport information.
    /// </summary>
    public required AirportInfo ArrivalAirport { get; set; }
    
    /// <summary>
    /// Arrival time
    /// </summary>
    public required DateTimeOffset ArrivalTime { get; set; }
}


public static class CheckInJobStateExtensions
{
    public static string ToSouthwestPrepareCheckInJobKey(this (string confirmationNumber, string firstName, string lastName, string departureAirport, DateTimeOffset departureTime) reservationInfo) => 
        $"southwest-prepare-checkin:{reservationInfo.ToCheckInJobKey()}";
    
    public static string ToSouthwestPerformCheckInJobKey(this (string confirmationNumber, string firstName, string lastName, string departureAirport, DateTimeOffset departureTime) reservationInfo) => 
        $"southwest-perform-checkin:{reservationInfo.ToCheckInJobKey()}";

    private static string ToCheckInJobKey( this (string confirmationNumber, string firstName, string lastName, string departureAirport, DateTimeOffset departureTime) reservationInfo) =>
        $"{reservationInfo.confirmationNumber}:{reservationInfo.firstName}:{reservationInfo.lastName}:{reservationInfo.departureAirport}:{reservationInfo.departureTime:MM-dd-HH-mm}".ToLower();
} 