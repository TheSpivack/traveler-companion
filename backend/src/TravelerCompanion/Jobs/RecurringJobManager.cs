using Hangfire;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;

namespace TravelerCompanion.Jobs;

public class RecurringJobManager<TModel, TJob>(IBackgroundJobStateRepository<TModel> repository, IRecurringJobManagerV2 recurringJobs, ILogger<RecurringJobManager<TModel, TJob>> logger) : IJobManager<TModel>
    where TModel : RecurringBaseJobState
    where TJob : BaseJob<TModel>
{
    public async Task CreateJobAsync(TModel model, bool triggerImmediately)
    {
        var cronStart = DateTime.UtcNow.AddMinutes(-2);
        model.CronExpression ??= Cron.Daily(cronStart.Hour, cronStart.Minute);
        
        logger.LogDebug("Creating recurring job {JobKey}: {CronExpression}",model.JobKey, model.CronExpression);
        recurringJobs.AddOrUpdate<TJob>(model.JobKey, job => job.RunJobAsync(model.JobKey), model.CronExpression);
        model.CurrentFriendlyDescription = "Idle";
        await repository.UpsertAsync(model);

        if (triggerImmediately)
        {
            logger.LogDebug("Triggering {JobKey} immediately", model.JobKey);
            recurringJobs.TriggerJob(model.JobKey);
        }
    }

    public async Task RemoveJobAsync(TModel model)
    {
        logger.LogDebug("Removing recurring job {JobKey}", model.JobKey);
        recurringJobs.RemoveIfExists(model.JobKey);
        await repository.DeleteAsync(model);
    }
}