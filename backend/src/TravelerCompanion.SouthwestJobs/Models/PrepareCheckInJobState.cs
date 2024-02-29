namespace TravelerCompanion.SouthwestJobs.Models;

/// <summary>
/// Job used to prepare the check-in for a Southwest flight
/// </summary>
public class PrepareCheckInJobState : CheckInJobState
{
    public override string JobKey =>
        (ConfirmationNumber, FirstName, LastName, DepartureAirport.Code, DepartureTime).ToSouthwestPrepareCheckInJobKey();
}