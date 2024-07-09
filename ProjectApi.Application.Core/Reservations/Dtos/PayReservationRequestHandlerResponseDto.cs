namespace ProjectApi.Application.Core.Reservations.Dtos;

public record PayReservationRequestHandlerResponseDto(
    string StripeSessionUrl
);