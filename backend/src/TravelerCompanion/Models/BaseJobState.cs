namespace TravelerCompanion.Models;

/// <summary>
/// Base class of all objects that will be used to track state of background jobs
/// </summary>
public abstract class BaseJobState
{
    /// <summary>
    /// Gets or sets a short friendly description of what the job is currently doing.
    /// </summary>
    public string CurrentFriendlyDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// The unique identifier for the related background job.  This will be the same as the hangfire id.
    /// </summary>
    public abstract string JobKey { get; }

    /// <summary>
    /// Two job states are equal if their JobKeys are the same
    /// </summary>
    public static bool operator ==(BaseJobState? x, BaseJobState? y)
    {
        return x?.Equals(y) == true;
    }

    public static bool operator !=(BaseJobState? x, BaseJobState? y)
    {
        return !(x == y);
    }

    public static bool operator ==(BaseJobState? x, string? y)
    {
        return x?.JobKey.Equals(y) == true;
    }

    public static bool operator !=(BaseJobState? x, string? y)
    {
        return !(x == y);
    }
    
    public static bool operator ==(string? x, BaseJobState? y)
    {
        return y?.JobKey.Equals(x) == true;
    }

    public static bool operator !=(string? x, BaseJobState? y)
    {
        return !(x == y);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        
        if (ReferenceEquals(this, null))
        {
            return false;
        }

        return obj switch
        {
            BaseJobState jobState => JobKey.Equals(jobState.JobKey),
            string jobKey => this == jobKey,
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return JobKey.GetHashCode();
    }
}