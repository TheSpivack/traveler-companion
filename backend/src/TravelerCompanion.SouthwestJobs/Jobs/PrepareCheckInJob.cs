using Hangfire;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Jobs;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs.Jobs;

public class PrepareCheckInJob(
    IBackgroundJobStateRepository<PrepareCheckInJobState> repository,
    ISouthwestWebDriver southwestWebDriver,
    IJobManager<PerformCheckInJobState> performCheckInJobManager,
    ILogger<SingleShotJob<PrepareCheckInJobState>> logger) : SingleShotJob<PrepareCheckInJobState>(repository, logger)
{
    private readonly IBackgroundJobStateRepository<PrepareCheckInJobState> _repository = repository;

    protected override async Task<PrepareCheckInJobState> RunJobAsync(PrepareCheckInJobState checkIn)
    {
        checkIn.CurrentFriendlyDescription = "Refreshing request headers";
        await _repository.UpsertAsync(checkIn);

        var headers = await southwestWebDriver.RefreshRequestHeadersAsync();

        await performCheckInJobManager.CreateJobAsync(new PerformCheckInJobState
        {
            ConfirmationNumber = checkIn.ConfirmationNumber,
            FirstName = checkIn.FirstName,
            LastName = checkIn.LastName,
            DepartureAirport = checkIn.DepartureAirport,
            DepartureTime = checkIn.DepartureTime,
            ArrivalAirport = checkIn.ArrivalAirport,
            RequestHeaders = headers,
            RunAt = checkIn.DepartureTime.AddDays(-1).AddSeconds(-5)
        });

        return checkIn;
    }
}

public class PrepareCheckInJobManager(
    IBackgroundJobStateRepository<PrepareCheckInJobState> repository, 
    IBackgroundJobClientV2 backgroundJobs, 
    ILogger<SingleShotJobManager<PrepareCheckInJobState, 
        PrepareCheckInJob>> logger) : SingleShotJobManager<PrepareCheckInJobState, PrepareCheckInJob>(repository, backgroundJobs, logger)
{
    
}
