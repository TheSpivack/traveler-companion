using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Models;

namespace TravelerCompanion.Repositories;

/// <summary>
/// Simple implementation of <see cref="IBackgroundJobStateRepository{TModel}"/> that stores the data in a concurrent dictionary.
/// Should be fine for our use case (not too many users, not too much data, no need for persistence beyond saving some crap in a config file, etc.)
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class InMemoryRepository<TModel> : IBackgroundJobStateRepository<TModel>
    where TModel : BaseJobState
{
    private readonly ConcurrentDictionary<string, TModel> _database = new();

    public Task<IQueryable<TModel>> GetAllAsync()
    {
        return Task.FromResult(_database.Values.AsQueryable());
    }

    public Task<TModel?> GetAsync(string jobKey)
    {
        return Task.FromResult(_database.GetValueOrDefault(jobKey));
    }

    public Task UpsertAsync(TModel model)
    {
        _database[model.JobKey] = model;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(TModel model)
    {
        return Task.FromResult(_database.Remove(model.JobKey, out _));
    }
}