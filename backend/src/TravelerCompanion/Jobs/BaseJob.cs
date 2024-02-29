using TravelerCompanion.Models;
using TravelerCompanion.Repositories;

namespace TravelerCompanion.Jobs;

/// <summary>
/// Base class for hangfire jobs that have state tracked in an <see cref="IBackgroundJobStateRepository{TModel}"/>
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class BaseJob<TModel>
    where TModel : BaseJobState
{
    /// <summary>
    /// The method called by hangfire.  Creates a scope for the logger, makes sure the state model can be retrieved,
    /// then runs the job.  After the job is complete, the state model is set back to completed, or idle
    /// if it is a <see cref="RecurringBaseJobState"/>. 
    /// </summary>
    public abstract Task RunJobAsync(string jobKey);
    
    
    /// <summary>
    /// The actual business logic of the job you are creating!
    /// </summary>
    protected abstract Task<TModel> RunJobAsync(TModel model);
}