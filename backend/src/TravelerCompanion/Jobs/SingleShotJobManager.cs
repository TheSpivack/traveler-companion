using Hangfire;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;

namespace TravelerCompanion.Jobs;

public class SingleShotJobManager<TModel, TJob>(IBackgroundJobStateRepository<TModel> repository, IBackgroundJobClientV2 backgroundJobs, ILogger<SingleShotJobManager<TModel, TJob>> logger) : IJobManager<TModel>
    where TModel : SingleShotJobState
    where TJob : BaseJob<TModel>
{
    public async Task CreateJobAsync(TModel model, bool triggerImmediately)
    {
        if (!triggerImmediately && model.RunAt.HasValue)
        {
            if (model.RunAt <= DateTimeOffset.Now)
            {
                throw new ApplicationException("RunAt cannot be in the past!");
            }
            logger.LogDebug("Queuing job {JobKey} to run at {RunAt}",model.JobKey, model.RunAt);
            model.HangfireJobId = backgroundJobs.Schedule<TJob>(job => job.RunJobAsync(model.JobKey), model.RunAt.Value);
            model.CurrentFriendlyDescription = "Queued";
            await repository.UpsertAsync(model);
        }
        else
        {
            logger.LogDebug("Queuing job {JobKey} to run in one second", model.JobKey);
            model.HangfireJobId = backgroundJobs.Schedule<TJob>(job => job.RunJobAsync(model.JobKey), TimeSpan.FromSeconds(1));
            model.CurrentFriendlyDescription = "Created";
            await repository.UpsertAsync(model);
        }
    }

    public async Task RemoveJobAsync(TModel model)
    {
        if (string.IsNullOrWhiteSpace(model.HangfireJobId))
        {
            throw new ApplicationException($"HangfireJobId is null. Cannot remove job {model.JobKey}");
        }
        
        logger.LogDebug("Removing job {JobKey} with HangfireJobId {HangfireJobId}", model.JobKey, model.HangfireJobId);
        backgroundJobs.Delete(model.HangfireJobId);
        await repository.DeleteAsync(model);
    }
}