namespace TravelerCompanion.Models;

public abstract class SingleShotJobState : BaseJobState
{
    /// <summary>
    /// The time when the job should run.
    /// If not set, will be queued to run immediately
    /// </summary>
    public DateTimeOffset? RunAt { get; set; }
    
    /// <summary>
    /// Hangfire won't let us specify job ids unless we're creating a recurring job, so we have to set it
    /// after it's created.
    /// </summary>
    public string? HangfireJobId { get; set; }
}