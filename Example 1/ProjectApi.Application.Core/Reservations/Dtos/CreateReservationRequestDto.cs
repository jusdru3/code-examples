using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Application.Core.Reservations.Dtos;

public record CreateReservationRequestDto(
    Guid SpotId,
    Guid BoatId,
    DateTimeRange DateTimeRange,
    bool? IncludeElectricity
);