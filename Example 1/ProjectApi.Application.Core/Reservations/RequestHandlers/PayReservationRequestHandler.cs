using AutoMapper;
using ProjectUtilities.Validation;
using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Application.Core.Reservations.Exceptions;
using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Application.Core.Users.Services.Interfaces;
using ProjectApi.Core.Stripe.Dtos;
using ProjectApi.Core.Stripe.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.RequestHandlers;

public class PayReservationRequestHandler : IPayReservationRequestHandler
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IMapper _mapper;
    private readonly ValidatorsAdapter _validatorsAdapter;
    private readonly IStripeService _stripeService;
    private readonly ICurrentApplicationUserStore _currentApplicationUserStore;

    public PayReservationRequestHandler(ApplicationDbContext applicationDbContext, IMapper mapper,
        ValidatorsAdapter validatorsAdapter, IStripeService stripeService,
        ICurrentApplicationUserStore currentApplicationUserStore)
    {
        _applicationDbContext = applicationDbContext;
        _mapper = mapper;
        _validatorsAdapter = validatorsAdapter;
        _stripeService = stripeService;
        _currentApplicationUserStore = currentApplicationUserStore;
    }

    public async Task<PayReservationRequestHandlerResponseDto> Handle(Guid id,
        PayReservationRequestHandlerRequestDto request)
    {
        var currentUser = await _currentApplicationUserStore.GetCurrentApplicationUser();

        if (!await _applicationDbContext.BillingDetailsInformations.AnyAsync(e =>
                e.ApplicationUserId == currentUser.Id))
        {
            throw ReservationException.NoBillingDetails();
        }

        var reservation = await _applicationDbContext.Reservations
            .ByUserId(currentUser.Id)
            .WithIncludes()
            .Where(e => e.Status == ReservationStatus.Draft)
            .FirstAsync(e => e.Id == id);

        var billingPeriod = reservation.BillingPeriods!
            .Where(e => e.PaymentStatus == ReservationBillingPeriodPaymentStatus.Pending)
            .OrderBy(e => e.DateTimeRangeInUtc.From)
            .First();


        var createCheckoutSessionRequestDto = new CreateCheckoutSessionRequestDto(
            currentUser.StripeCustomerId,
            new Dictionary<string, string>()
            {
                { "reservationId", reservation.Id.ToString() },
                { "billingPeriodId", billingPeriod.Id.ToString() }
            },
            null,
            reservation.IsPaidOverTime(),
            new List<CreateCheckoutSessionRequestLineItemDto>()
            {
                new("Spot Fee", billingPeriod.Cost.Amount, 0),
                new("Service Fee", billingPeriod.Cost.ServiceFee, 0)
            });

        var stripeCheckoutSession = await _stripeService.CreateCheckoutSession(createCheckoutSessionRequestDto);

        return new PayReservationRequestHandlerResponseDto(stripeCheckoutSession.SessionUrl);
    }
}