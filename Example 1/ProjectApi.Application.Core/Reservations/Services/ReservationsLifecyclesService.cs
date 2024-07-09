using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Reservations.OwnedTypes;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.Services;

public class ReservationsLifecyclesService : IReservationsLifecyclesService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReservationsLifecyclesService(ApplicationDbContext applicationDbContext, IDateTimeProvider dateTimeProvider)
    {
        _applicationDbContext = applicationDbContext;
        _dateTimeProvider = dateTimeProvider;
    }


    public async Task CompleteReservations()
    {
        var reservations = await _applicationDbContext.Reservations
            .Include(e => e.Spot)
            .Where(e => e.Status == ReservationStatus.Started)
            .Where(e => e.DateTimeRangeInUtc.To <= _dateTimeProvider.NowUtc())
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            reservation.ChangeToCompleted();
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task StartReservations()
    {
        var reservations = await _applicationDbContext.Reservations
            .Include(e => e.Spot)
            .Where(e => e.Status == ReservationStatus.Confirmed)
            .Where(e => e.DateTimeRangeInUtc.From <= _dateTimeProvider.NowUtc())
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            reservation.ChangeToStarted();
        }

        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task TerminateReservations()
    {
        var reservationBillingPeriods = await _applicationDbContext.ReservationBillingPeriods
            .Where(e => e.PaymentStatus == ReservationBillingPeriodPaymentStatus.Failed)
            .Where(e => e.Reservation!.Status == ReservationStatus.Started)
            .Where(e => e.DateTimeRangeInUtc.From <= _dateTimeProvider.NowUtc())
            .Include(e => e.Reservation!)
            .ToListAsync();

        foreach (var reservation in reservationBillingPeriods.Select(e => e.Reservation!))
        {
            reservation.ChangeToTerminated(_dateTimeProvider.NowUtc(),
                ReservationTerminationReason.MultiplePaymentAttemptsFailed);
        }

        await _applicationDbContext.SaveChangesAsync();
    }
}