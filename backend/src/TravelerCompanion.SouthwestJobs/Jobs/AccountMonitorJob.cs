using Hangfire;
using Microsoft.Extensions.Logging;
using TravelerCompanion.Jobs;
using TravelerCompanion.Repositories;
using TravelerCompanion.SouthwestJobs.Models;

namespace TravelerCompanion.SouthwestJobs.Jobs;

public class AccountMonitorJob(
    IBackgroundJobStateRepository<AccountMonitorState> accountsRepository, 
    IBackgroundJobStateRepository<ReservationMonitorState> reservationsRepository,
    IJobManager<ReservationMonitorState> reservationsManager,
    ISouthwestWebDriver southwestWebDriver,
    ILogger<AccountMonitorJob> logger) : RecurringJob<AccountMonitorState>(accountsRepository, logger)
{
    private readonly IBackgroundJobStateRepository<AccountMonitorState> _accountsRepository = accountsRepository;
    
    protected override async Task<AccountMonitorState> RunJobAsync(AccountMonitorState account)
    {
        account.CurrentFriendlyDescription = "Refreshing reservations";
        await _accountsRepository.UpsertAsync(account);

        logger.LogDebug("Retrieving account and refreshing reservations");
        var result = await southwestWebDriver.RetrieveAccountAndReservationsAsync(account.Username, account.Password ?? "");
        account.AccountNumber = result.account.AccountNumber;
        account.FirstName = result.account.FirstName;
        account.LastName = result.account.LastName;
        account.RedeemablePoints = result.account.RedeemablePoints;
        var reservations = result.reservations;

        logger.LogDebug("Saving account");
        await _accountsRepository.UpsertAsync(account);

        var newReservations = reservations.ToList();
        logger.LogDebug("Successfully retrieved {ReservationCount} reservations", newReservations);

        var existingReservations = (await reservationsRepository.GetAllAsync())
            .Where(r => account.Username.Equals(r.AccountUsername, StringComparison.OrdinalIgnoreCase));

        var reservationsToRemove = existingReservations.Except(newReservations).ToList();
        var reservationsToAdd = newReservations.Except(existingReservations).ToList();

        logger.LogDebug("Removing {ReservationCount} old reservations", reservationsToRemove.Count);
        foreach (var reservation in reservationsToRemove.Cast<ReservationMonitorState>())
        {
            await reservationsManager.RemoveJobAsync(reservation);
        }

        logger.LogDebug("Removing {ReservationCount} old reservations", reservationsToRemove.Count);
        foreach (var reservation in reservationsToAdd.Cast<ReservationMonitorState>())
        {
            logger.LogDebug("Creating reservation monitor for {ConfirmationNumber}", reservation.ConfirmationNumber);
            await reservationsManager.CreateJobAsync(reservation, true);
        }
        return account;
    }
}

public class AccountMonitorJobManager(
    IBackgroundJobStateRepository<AccountMonitorState> repository,
    IRecurringJobManagerV2 recurringJobs,
    ILogger<AccountMonitorJobManager> logger)
    : RecurringJobManager<AccountMonitorState, AccountMonitorJob>(repository, recurringJobs, logger)
{
}