namespace TravelerCompanion.SouthwestJobs.Models;

public interface ISouthwestBaseJobState
{
    public IDictionary<string, string> RequestHeaders { get; set; }
}