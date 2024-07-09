using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectUtilities.Events.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Reservations.Events.ReservationBillingPeriods;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.Services;

public class ReservationsChargingService : IReservationsChargingService
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEventsBus _eventsBus;

    public ReservationsChargingService(ApplicationDbContext applicationDbContext, IDateTimeProvider dateTimeProvider,
        IEventsBus eventsBus)
    {
        _applicationDbContext = applicationDbContext;
        _dateTimeProvider = dateTimeProvider;
        _eventsBus = eventsBus;
    }

    public async Task ChargeDueReservations()
    {
        var nowUtc = _dateTimeProvider.NowUtc();
        var timeSpanForFirstCharge = TimeSpan.FromDays(5);

        var billingPeriods = await _applicationDbContext.ReservationBillingPeriods
            .Include(e => e.Reservation)
            .Where(e => e.DateTimeRangeInUtc.From - nowUtc <= timeSpanForFirstCharge)
            .Where(e => e.Reservation!.Status == ReservationStatus.Started)
            .Where(e => e.PaymentStatus == ReservationBillingPeriodPaymentStatus.Pending)
            .Where(e => !e.BillingPeriodPayments!.Any())
            .ToListAsync();

        foreach (var billingPeriod in billingPeriods)
        {
            await _eventsBus.RaiseEvent(new ReservationBillingPeriodChargeDueEvent(billingPeriod.Id));
        }
    }

    public async Task RetryPendingReservations()
    {
        var nowUtc = _dateTimeProvider.NowUtc();

        var billingPeriods = await _applicationDbContext.ReservationBillingPeriods
            .Where(e => e.ChargeRetries!.NextRetry <= nowUtc)
            .ToListAsync();

        foreach (var billingPeriod in billingPeriods)
        {
            await _eventsBus.RaiseEvent(new ReservationBillingPeriodChargeRetryDueEvent(billingPeriod.Id));
        }
    }
}