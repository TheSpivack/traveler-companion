using TravelerCompanion.Models;

namespace TravelerCompanion.Jobs;

public interface IJobManager<in TModel>
    where TModel : BaseJobState
{
    /// <summary>
    /// Creates a job in hangfire and saves the state in the repository
    /// </summary>
    Task CreateJobAsync(TModel model, bool triggerImmediately = false);
    
    /// <summary>
    /// Creates a job in hangfire and saves the state in the repository
    /// </summary>
    Task RemoveJobAsync(TModel model);
}