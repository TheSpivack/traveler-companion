using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;

namespace TravelerCompanion.Jobs;

/// <summary>
/// Base class for jobs that are recurring
/// </summary>
public abstract class RecurringJob<TModel>(
    IBackgroundJobStateRepository<TModel> repository,
    ILogger<RecurringJob<TModel>> logger) : BaseJob<TModel>
    where TModel : RecurringBaseJobState
{
    public override async Task RunJobAsync(string jobKey)
    {
        using (logger.BeginScope(new Dictionary<string, string>
               {
                   ["JobKey"] = jobKey
               }))
        {
            logger.LogDebug("Job starting");
            var model = await repository.GetAsync(jobKey);
            if (model is null)
            {
                throw new ApplicationException($"Cannot locate {jobKey} in the repository");
            }
            model.CurrentFriendlyDescription = "Running";
            await repository.UpsertAsync(model);
    
    
            model = await RunJobAsync(model);
    
            model.CurrentFriendlyDescription = "Idle";
            await repository.UpsertAsync(model);
            logger.LogDebug("Job completed successfully");
        }
    }   
}