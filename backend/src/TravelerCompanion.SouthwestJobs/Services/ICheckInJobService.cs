using TravelerCompanion.Models;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs.Services;

public interface ICheckInJobService
{
    /// <summary>
    /// Gets the existing check-in jobs for the given reservation info, and optionally the flight bounds
    /// </summary>
    Task<IEnumerable<CheckInJobState>> GetExistingCheckInJobsAsync(ReservationMonitorState reservation, IEnumerable<FlightBoundInformation>? bounds = null);
    
    /// <summary>
    /// Creates a check-in job state for the given reservation and flight bound.  If the flight is within 20 minutes, it will be a
    /// <see cref="PerformCheckInJobState"/>, otherwise it will be a <see cref="PrepareCheckInJobState"/>
    /// </summary>
    CheckInJobState CreateCheckInJobState(ReservationMonitorState reservation, FlightBoundInformation flightBound);
}