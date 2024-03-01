using TravelerCompanion.Models;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs.Services;

public class CheckInJobService(IBackgroundJobStateRepository<PrepareCheckInJobState> prepareRepository, IBackgroundJobStateRepository<PerformCheckInJobState> peformRepository) : ICheckInJobService
{
    public async Task<IEnumerable<CheckInJobState>> GetExistingCheckInJobsAsync(ReservationMonitorState reservation, IEnumerable<FlightBoundInformation>? bounds = null)
    {
        var prepareJobs = (await prepareRepository.GetAllAsync()).Where(j =>
            j.ConfirmationNumber.Equals(reservation.ConfirmationNumber, StringComparison.OrdinalIgnoreCase)
            && j.FirstName.Equals(reservation.FirstName, StringComparison.OrdinalIgnoreCase)
            && j.LastName.Equals(reservation.LastName, StringComparison.OrdinalIgnoreCase)); 
        
        var performJobs = (await peformRepository.GetAllAsync()).Where(j =>
            j.ConfirmationNumber.Equals(reservation.ConfirmationNumber, StringComparison.OrdinalIgnoreCase)
            && j.FirstName.Equals(reservation.FirstName, StringComparison.OrdinalIgnoreCase)
            && j.LastName.Equals(reservation.LastName, StringComparison.OrdinalIgnoreCase));

        if (bounds != null)
        {
            prepareJobs = prepareJobs.Where(j => bounds.Select(b => b.DepartureAirport.Code).Contains(j.DepartureAirport.Code));
            performJobs = performJobs.Where(j => bounds.Select(b => b.DepartureAirport.Code).Contains(j.DepartureAirport.Code));
        }

        var ret = new List<CheckInJobState>();
        ret.AddRange(prepareJobs);
        ret.AddRange(performJobs);
        return ret;
    }


    public CheckInJobState CreateCheckInJobState(ReservationMonitorState reservation,
        FlightBoundInformation flightBound)
    {
        return flightBound.DepartureTime < DateTimeOffset.Now.AddHours(24).AddMinutes(20)
            ? new PerformCheckInJobState
            {
                ConfirmationNumber = reservation.ConfirmationNumber,
                FirstName = reservation.FirstName,
                LastName = reservation.LastName,
                DepartureAirport = flightBound.DepartureAirport,
                DepartureTime = flightBound.DepartureTime,
                ArrivalAirport = flightBound.ArrivalAirport,
                RequestHeaders = reservation.RequestHeaders,
                RunAt = flightBound.DepartureTime.AddHours(-24).AddSeconds(-5)
            }
            : new PrepareCheckInJobState
            {
                ConfirmationNumber = reservation.ConfirmationNumber,
                FirstName = reservation.FirstName,
                LastName = reservation.LastName,
                DepartureAirport = flightBound.DepartureAirport,
                DepartureTime = flightBound.DepartureTime,
                ArrivalAirport = flightBound.ArrivalAirport,
                RunAt = flightBound.DepartureTime.AddHours(-24).AddMinutes(-20)
            };

    }
}