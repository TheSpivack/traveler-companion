namespace TravelerCompanion.Models;

public abstract class RecurringBaseJobState : BaseJobState
{
    /// <summary>
    /// Cron expression to use for this recurring job.
    /// If not set, will default to daily at the time when the job gets created
    /// </summary>
    public string? CronExpression { get; set; }
}