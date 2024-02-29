using Microsoft.AspNetCore.Mvc;
using TravelerCompanion.Jobs;
using TravelerCompanion.Models;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.Host.Controllers.Southwest;

[Route("southwest/reservation-monitors")]
public class ReservationMonitorsController(IBackgroundJobStateRepository<ReservationMonitorState> repository, IJobManager<ReservationMonitorState> jobManager) : Controller
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ReservationMonitorState>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Json(await repository.GetAllAsync());
    }
    
    [HttpGet("{confirmationNumber}")]
    [ProducesResponseType<ReservationMonitorState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(string confirmationNumber, [FromQuery] string firstName, [FromQuery] string lastName)
    {
        var reservation = await repository.GetAsync((confirmationNumber, firstName, lastName).ToSouthwestReservationMonitorJobKey());
        if (reservation is null)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status404NotFound, $"Reservation #{confirmationNumber} for {firstName} {lastName} not found"));
        }
        
        return Json(await repository.GetAllAsync());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNew(string confirmationNumber, string firstName, string lastName, bool triggerImmediately)
    {
        await jobManager.CreateJobAsync(new ReservationMonitorState
        {
            ConfirmationNumber = confirmationNumber,
            FirstName = firstName,
            LastName = lastName
        }, triggerImmediately);
        return CreatedAtAction("GetDetails", new { confirmationNumber, firstName, lastName }, null);
    }
    
    [HttpDelete("{confirmationNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(string confirmationNumber, [FromQuery] string firstName, [FromQuery] string lastName)
    {
        var reservation = await repository.GetAsync((confirmationNumber, firstName, lastName).ToSouthwestReservationMonitorJobKey());
        if (reservation is null)
        {
            return NotFound(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status404NotFound, $"Reservation #{confirmationNumber} for {firstName} {lastName} not found"));
        }
        
        await jobManager.RemoveJobAsync(reservation);
        return NoContent();
    }
}
