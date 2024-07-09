using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.Services;

public class ReservationsCleanupService : IReservationsCleanupService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ApplicationDbContext _applicationDbContext;

    public ReservationsCleanupService(IDateTimeProvider dateTimeProvider,
        ApplicationDbContext applicationDbContext)
    {
        _dateTimeProvider = dateTimeProvider;
        _applicationDbContext = applicationDbContext;
    }

    public async Task ExpireDrafts()
    {
        var reservations = await _applicationDbContext.Reservations
            .Where(e => e.Status == ReservationStatus.Draft)
            .Where(e => e.DraftExpirationTime <= _dateTimeProvider.NowUtc())
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            reservation.ChangeToPaymentTimedOut();
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task ExpireApprovals()
    {
        var now = _dateTimeProvider.NowUtc();
        var expiredApprovals = await _applicationDbContext.ReservationApprovals
            .Include(e => e.Reservation)
            .Where(e => e.Status == null)
            .Where(e => e.ExpireAt <= now)
            .ToListAsync();

        foreach (var reservationApproval in expiredApprovals)
        {
            reservationApproval.Expire(_dateTimeProvider.NowUtc());
        }

        await _applicationDbContext.SaveChangesAsync();
    }
}