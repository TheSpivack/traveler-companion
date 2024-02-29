using Microsoft.Extensions.Logging;
using TravelerCompanion.Jobs;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs.Jobs;

public class PerformCheckInJob(
    IBackgroundJobStateRepository<PerformCheckInJobState> repository,
    ISouthwestWebDriver southwestWebDriver,
    ILogger<SingleShotJob<PerformCheckInJobState>> logger) : SingleShotJob<PerformCheckInJobState>(repository, logger)
{
    private readonly IBackgroundJobStateRepository<PerformCheckInJobState> _repository = repository;
    
    protected override Task<PerformCheckInJobState> RunJobAsync(PerformCheckInJobState model)
    {
        throw new NotImplementedException();
    }
}