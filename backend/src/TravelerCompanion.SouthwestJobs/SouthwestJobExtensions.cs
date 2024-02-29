using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;
using TravelerCompanion.Jobs;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Jobs;
using TravelerCompanion.SouthwestJobs.Models;
using TravelerCompanion.SouthwestJobs.Services;

namespace TravelerCompanion.SouthwestJobs;

public static class SouthwestJobExtensions
{
    /// <summary>
    /// Adds the Job managers for all the Southwest jobs 
    /// </summary>
    public static IServiceCollection AddSouthwestJobs(this IServiceCollection services)
    {
        services.AddSingleton<IJobManager<AccountMonitorState>, AccountMonitorJobManager>();
        services.AddSingleton<IJobManager<ReservationMonitorState>, ReservationMonitorJobManager>();
        services.AddSingleton<IJobManager<PrepareCheckInJobState>, PrepareCheckInJobManager>();
        services.AddSingleton<IJobManager<PerformCheckInJobState>, PerformCheckInJobManager>();
        services.AddSingleton<ICheckInJobService, CheckInJobService>();
        return services;
    }

    /// <summary>
    /// Adds the in-memroy repositories for the Southwest jobs 
    /// </summary>
    public static IServiceCollection AddSouthwestInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundJobStateRepository<AccountMonitorState>, InMemoryRepository<AccountMonitorState>>();
        services.AddSingleton<IBackgroundJobStateRepository<ReservationMonitorState>, InMemoryRepository<ReservationMonitorState>>();
        services.AddSingleton<IBackgroundJobStateRepository<PrepareCheckInJobState>, InMemoryRepository<PrepareCheckInJobState>>();
        services.AddSingleton<IBackgroundJobStateRepository<PerformCheckInJobState>, InMemoryRepository<PerformCheckInJobState>>();
        return services;
    }
    
    /// <summary>
    /// Adds the <see cref="ISouthwestWebDriver"/> to the service collection and downloads the required browser for puppeteer.
    /// </summary>
    public static async Task<IServiceCollection> AddSouthwestWebDriverAsync(this IServiceCollection services, bool enableApiLogging = false)
    {
        if (enableApiLogging)
        {
            services.AddTransient<LoggingHandler>();    
        }
        var httpBuilder = services.AddHttpClient(nameof(SouthwestWebDriver));
        if (enableApiLogging)
        {
            httpBuilder.AddHttpMessageHandler<LoggingHandler>();
        }
            
        services.AddScoped<ISouthwestWebDriver, SouthwestWebDriver>();
        
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        
        return services;
    }
}