using Hangfire;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Jobs;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;
using TravelerCompanion.SouthwestJobs.Services;

namespace TravelerCompanion.SouthwestJobs.Jobs;

public class ReservationMonitorJob(
    IBackgroundJobStateRepository<ReservationMonitorState> reservationsRepository,
    ICheckInJobService checkInJobService,
    IJobManager<PrepareCheckInJobState> prepareCheckInManager,
    IJobManager<PerformCheckInJobState> performCheckInManager,
    ISouthwestWebDriver southwestWebDriver,
    ILogger<ReservationMonitorJob> logger) : RecurringJob<ReservationMonitorState>(reservationsRepository, logger)
{
    private readonly IBackgroundJobStateRepository<ReservationMonitorState> _reservationsRepository =
        reservationsRepository;

    protected override async Task<ReservationMonitorState> RunJobAsync(ReservationMonitorState reservation)
    {
        reservation.CurrentFriendlyDescription = "Refreshing flights";
        await _reservationsRepository.UpsertAsync(reservation);

        logger.LogDebug("Retrieving flight bounds");
        var flightBounds = await southwestWebDriver.RetrieveFlightBoundsAsync(reservation);

        var newBounds = flightBounds.ToList();
        logger.LogDebug("Successfully retrieved {FlightCount} flight bounds", newBounds.Count);

        
        var existingCheckInJobs = (await checkInJobService.GetExistingCheckInJobsAsync(reservation)).ToList();
        var newCheckInJobs = newBounds.Select(bound => checkInJobService.CreateCheckInJobState(reservation, bound)).ToList();

        var jobsToRemove = existingCheckInJobs.Except(newCheckInJobs).ToList();
        var jobsToAdd = newCheckInJobs.Except(existingCheckInJobs).ToList();
        var jobsToDoNothing = existingCheckInJobs.Intersect(newCheckInJobs).ToList();
        
        logger.LogDebug("Found {CheckInJobCount} up-to-date check-in jobs for this reservation", jobsToDoNothing.Count);
        await RemoveCheckInJobsAsync(jobsToRemove);
        await AddCheckInJobsAsync(jobsToAdd);
        
        return reservation;
    }

    internal async Task RemoveCheckInJobsAsync(IList<CheckInJobState> jobs)
    {
        logger.LogDebug("Removing {CheckInJobCount} old check-in jobs for this reservation", jobs.Count);
        foreach (var job in jobs)
        {
            switch (job)
            {
                case PerformCheckInJobState performJob:
                    await performCheckInManager.RemoveJobAsync(performJob);
                    break;
                
                case PrepareCheckInJobState prepareJob:
                    await prepareCheckInManager.RemoveJobAsync(prepareJob);
                    break;
                
                default:
                    logger.LogWarning("Uh oh!  Don't know to remove a {JobStateType}", job.GetType().Name);
                    break;
                    
            }
        }
    }

    internal async Task AddCheckInJobsAsync(IList<CheckInJobState> jobs)
    {
        logger.LogDebug("Adding {CheckInJobCount} new check-in jobs for this reservation", jobs.Count);
        foreach (var job in jobs)
        {
            switch (job)
            {
                case PerformCheckInJobState performJob:
                    logger.LogDebug("Check-in for flight departing {DepartureAirport} is less than 20 minutes.  Creating perform check-in job", job.DepartureAirport.Code);
                    await performCheckInManager.CreateJobAsync(performJob);
                    break;
                
                case PrepareCheckInJobState prepareJob:
                    logger.LogDebug("Creating prepare check-in job for flight departing {DepartureAirport}", job.DepartureAirport.Code);
                    await prepareCheckInManager.CreateJobAsync(prepareJob);
                    break;
                
                default:
                    logger.LogWarning("Uh oh!  Don't know how to add a {JobStateType}", job.GetType().Name);
                    break;
            }
        }
    }
}

public class ReservationMonitorJobManager(
    IBackgroundJobStateRepository<ReservationMonitorState> repository,
    IRecurringJobManagerV2 recurringJobs,
    ILogger<ReservationMonitorJobManager> logger)
    : RecurringJobManager<ReservationMonitorState, ReservationMonitorJob>(repository, recurringJobs, logger)
{
}



