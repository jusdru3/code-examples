using ProjectUtilities.Events.Services.Interfaces;
using ProjectApi.Core.Stripe.Dtos;
using ProjectApi.Core.Stripe.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Reservations.Events;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.EventHandlers.ReservationCanceledEventHandlers;

public class RefundAfterCanceledReservationEventHandler : IEventHandler<ReservationCanceledEvent>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IStripeService _stripeService;

    public RefundAfterCanceledReservationEventHandler(ApplicationDbContext applicationDbContext,
        IStripeService stripeService)
    {
        _applicationDbContext = applicationDbContext;
        _stripeService = stripeService;
    }

    public async Task Handle(ReservationCanceledEvent @event)
    {
        var reservation = await _applicationDbContext.Reservations
            .Include(e => e.Cancelation)
            .Include(e => e.ApplicationUser!.IdentityUser)
            .FirstAsync(e => e.Id == @event.ReservationId && e.Status == ReservationStatus.Canceled);

        var refundAmount = reservation.Cancelation!.RefundAmount;

        if (refundAmount == 0) return;

        var billingPeriod = await _applicationDbContext.ReservationBillingPeriods
            .Include(e => e.BillingPeriodPayments)
            .Where(e => e.PaymentStatus == ReservationBillingPeriodPaymentStatus.Paid ||
                        e.PaymentStatus == ReservationBillingPeriodPaymentStatus.PaidOut)
            .SingleAsync(e => e.ReservationId == reservation.Id);

        var billingPeriodPayment =
            billingPeriod.BillingPeriodPayments!.Single(e => e.StripePaymentIntentStatus == "succeeded");

        var refundResponse = await _stripeService.CreateRefund(
            new CreateRefundRequestDto(billingPeriodPayment.StripePaymentIntentId, (int)refundAmount * 100));

        reservation.Cancelation!.StripeRefundId = refundResponse.RefundId;
        reservation.Cancelation!.StripeRefundStatus = refundResponse.Status;

        await _applicationDbContext.SaveChangesAsync();
    }
}