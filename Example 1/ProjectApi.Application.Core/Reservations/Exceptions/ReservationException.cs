using ProjectUtilities.Utilities.Exceptions;

namespace ProjectApi.Application.Core.Reservations.Exceptions;

public class ReservationException : Exception, IUserFriendlyException
{
    private ReservationException(string message) : base(message)
    {
    }

    public static ReservationException AlreadyReservedOrNotFound() =>
        new("The reservation cannot be processed because it is either already reserved or not found.");

    public static ReservationException InsufficientNotice() => new(
        "Your chosen start time is less than 1 hour away. Please revise it to give the host sufficient time to respond.");

    public static ReservationException UserIsSpotOwner() => new("You cannot reserve your own spot.");
    public static ReservationException NoBillingDetails() => new("Please provide your billing details to proceed.");

    public static ReservationException SpotOwnerDontHaveStripeConnectedAccount() => new(
        "This is a new listing. The project team is currently reviewing it. Please come back shortly!");

    public static ReservationException SpotBookingDisabled() =>
        new("This is a new listing. The project team is currently reviewing it. Please come back shortly!");
}