using Newtonsoft.Json;
using TravelerCompanion.Models;

namespace TravelerCompanion.SouthwestJobs.Models;

public class ReservationMonitorState : RecurringBaseJobState, ISouthwestBaseJobState
{
    public override string JobKey => (ConfirmationNumber, FirstName, LastName).ToSouthwestReservationMonitorJobKey();

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
    /// If present, the username of the account that was used to initiate this reservation
    /// </summary>
    public string? AccountUsername { get; set; }
    
    [JsonIgnore]
    public IDictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
}

public static class ReservationMonitorStateExtensions
{
    public static string ToSouthwestReservationMonitorJobKey(this (string confirmationNumber, string firstName, string lastName) reservationInfo) => 
        $"southwest-reservation:{reservationInfo.confirmationNumber}:{reservationInfo.firstName}:{reservationInfo.lastName}".ToLower();
}