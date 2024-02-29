using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;

namespace TravelerCompanion.Jobs;

/// <summary>
/// Base class for jobs that will run only once
/// </summary>
public abstract class SingleShotJob<TModel>(
    IBackgroundJobStateRepository<TModel> repository,
    ILogger<SingleShotJob<TModel>> logger) : BaseJob<TModel>
    where TModel : SingleShotJobState
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
    
            model.CurrentFriendlyDescription = "Completed";
            await repository.UpsertAsync(model);
            logger.LogDebug("Job completed successfully");
        }
    }   
}