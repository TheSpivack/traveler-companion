using System.Text.Json.Serialization;
using TravelerCompanion.Models;

namespace TravelerCompanion.SouthwestJobs.Models;

public class AccountMonitorState : RecurringBaseJobState, ISouthwestBaseJobState
{
    public override string JobKey => Username.ToSouthwestAccountMonitorJobKey();
    
    /// <summary>
    /// Username of southwest account
    /// </summary>
    public required string Username { get; init; }
    
    /// <summary>
    /// Password of southwest account
    /// </summary>
    [JsonIgnore]
    public string? Password { get; init; }
    
    /// <summary>
    /// Rapid rewards number.  This will be populated after the account monitor job runs.
    /// </summary>
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// First name.  This will be populated after the account monitor job runs.
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Last name.  This will be populated after the account monitor job runs.
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Redeemable points.  This will be populated after the account monitor job runs.
    /// </summary>
    public int? RedeemablePoints { get; set; }

    [JsonIgnore]
    public IDictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
}

public static class AccountMonitorStateExtensions
{
    public static string ToSouthwestAccountMonitorJobKey(this string username) => $"southwest-account:{username}".ToLower();
}