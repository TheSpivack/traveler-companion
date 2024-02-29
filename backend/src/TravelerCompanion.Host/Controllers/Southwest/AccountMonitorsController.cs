using Microsoft.AspNetCore.Mvc;
using TravelerCompanion.Jobs;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.Host.Controllers.Southwest;

[Route("southwest/account-monitors")]
public class AccountMonitorsController(IBackgroundJobStateRepository<AccountMonitorState> repository, IJobManager<AccountMonitorState> jobManager) : Controller
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<AccountMonitorState>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Json(await repository.GetAllAsync());
    }
    
    [HttpGet("{username}")]
    [ProducesResponseType<AccountMonitorState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(string username)
    {
        var account = await repository.GetAsync(username.ToSouthwestAccountMonitorJobKey());
        if (account is null)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status404NotFound, $"Account {username} not found"));
        }
        
        return Json(await repository.GetAllAsync());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNew(string username, string password, bool triggerImmediately)
    {
        await jobManager.CreateJobAsync(new AccountMonitorState
        {
            Username = username,
            Password = password
        }, triggerImmediately);
        return CreatedAtAction("GetDetails", new { username }, null);
    }
    
    [HttpDelete("{username}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(string username)
    {
        var account = await repository.GetAsync(username.ToSouthwestAccountMonitorJobKey());
        if (account is null)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status404NotFound, $"Account {username} not found"));
        }
        
        await jobManager.RemoveJobAsync(account);
        return NoContent();
    }
}
