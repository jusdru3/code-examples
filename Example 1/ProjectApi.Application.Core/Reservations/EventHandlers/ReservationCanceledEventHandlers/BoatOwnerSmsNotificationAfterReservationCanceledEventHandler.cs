using ProjectUtilities.Events.Services.Interfaces;
using ProjectApi.Core.PhoneMessages.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations.Events;
using ProjectApi.Data.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.EventHandlers.ReservationCanceledEventHandlers;

public class BoatOwnerSmsNotificationAfterReservationCanceledEventHandler : IEventHandler<ReservationCanceledEvent>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IPhoneMessageService _phoneMessageService;

    public BoatOwnerSmsNotificationAfterReservationCanceledEventHandler(ApplicationDbContext applicationDbContext,
        IPhoneMessageService phoneMessageService)
    {
        _applicationDbContext = applicationDbContext;
        _phoneMessageService = phoneMessageService;
    }

    public async Task Handle(ReservationCanceledEvent @event)
    {
        var reservation = await _applicationDbContext.Reservations
            .Include(e => e.ApplicationUser!.IdentityUser)
            .Include(e => e.Spot)
            .Include(e => e.Cancelation)
            .FirstAsync(e => e.Id == @event.ReservationId);

        var boatOwner = reservation.ApplicationUser!;

        if (boatOwner.Id == reservation.Cancelation!.RequestedById)
        {
            return;
        }

        await _phoneMessageService.SendPhoneMessage(
            $"Your reservation at {reservation.Spot!.Title} has been canceled. Open project app to view more details.",
            new PhoneNumber(boatOwner.IdentityUser!.PhoneNumber));
    }
}