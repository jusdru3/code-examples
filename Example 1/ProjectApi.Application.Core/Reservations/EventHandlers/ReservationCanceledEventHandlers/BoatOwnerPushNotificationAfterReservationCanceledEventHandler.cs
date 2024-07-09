using ProjectUtilities.Events.Services.Interfaces;
using ProjectApi.Core.FireBase.Dtos;
using ProjectApi.Core.FireBase.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations.Events;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.EventHandlers.ReservationCanceledEventHandlers;

public class BoatOwnerPushNotificationAfterReservationCanceledEventHandler : IEventHandler<ReservationCanceledEvent>
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IFireBaseNotificationsService _fireBaseNotificationsService;

    public BoatOwnerPushNotificationAfterReservationCanceledEventHandler(ApplicationDbContext applicationDbContext,
        IFireBaseNotificationsService fireBaseNotificationsService)
    {
        _applicationDbContext = applicationDbContext;
        _fireBaseNotificationsService = fireBaseNotificationsService;
    }

    public async Task Handle(ReservationCanceledEvent @event)
    {
        var reservation = await _applicationDbContext.Reservations
            .Include(e => e.Spot)
            .Include(e => e.ApplicationUser)
            .Include(e => e.Cancelation)
            .FirstAsync(e => e.Id == @event.ReservationId);

        var boatOwner = reservation.ApplicationUser!;

        if (boatOwner.Id == reservation.Cancelation!.RequestedById)
        {
            return;
        }

        await _fireBaseNotificationsService.SendNotification(boatOwner.IdentityUserId,
            new SendNotificationRequestDto(
                "Reservation Canceled",
                $"Your reservation at {reservation.Spot!.Title} has been canceled. Click here for more details.",
                NotificationType.ReservationCanceledBySpotOwner,
                new Dictionary<string, string>()
                {
                    { "ReservationId", reservation.Id.ToString() },
                    { "SpotId", reservation.SpotId.ToString() }
                }));
    }
}