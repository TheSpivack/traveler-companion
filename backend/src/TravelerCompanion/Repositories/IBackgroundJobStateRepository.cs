using TravelerCompanion.Models;

namespace TravelerCompanion.Repositories;

/// <summary>
/// Interface defining the repository for background job state
/// </summary>
public interface IBackgroundJobStateRepository<TModel>
    where TModel : BaseJobState
{
    /// <summary>
    /// Gets all of the job state objects
    /// </summary>
    public Task<IQueryable<TModel>> GetAllAsync();

    /// <summary>
    /// Gets the job state by job key.  Returns null if not found.
    /// </summary>
    public Task<TModel?> GetAsync(string jobKey);
    
    /// <summary>
    /// Updates or inserts a job state object
    /// </summary>
    public Task UpsertAsync(TModel model);
    
    /// <summary>
    /// Removes a job state object. Returns true if the object was successfully removed.
    /// </summary>
    public Task<bool> DeleteAsync(TModel model);
}