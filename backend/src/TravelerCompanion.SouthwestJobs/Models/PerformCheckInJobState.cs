using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TravelerCompanion.Jobs;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Jobs;

namespace TravelerCompanion.SouthwestJobs.Models;

/// <summary>
/// Job used to perform the check-in for a Southwest flight
/// </summary>
public class PerformCheckInJobState : CheckInJobState, ISouthwestBaseJobState
{
    public override string JobKey =>
        (ConfirmationNumber, FirstName, LastName, DepartureAirport.Code, DepartureTime).ToSouthwestPerformCheckInJobKey();
    
    [JsonIgnore]
    public IDictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
}

public class PerformCheckInJobManager(
    IBackgroundJobStateRepository<PerformCheckInJobState> repository, 
    IBackgroundJobClientV2 backgroundJobs, 
    ILogger<SingleShotJobManager<PerformCheckInJobState, 
        PerformCheckInJob>> logger) : SingleShotJobManager<PerformCheckInJobState, PerformCheckInJob>(repository, backgroundJobs, logger)
{
    
}