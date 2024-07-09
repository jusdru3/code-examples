using ProjectApi.Data.Models.Reservations;

namespace ProjectApi.Application.Core.Reservations.Dtos;

public record GetReservationCostResponseDto(ReservationCost ReservationCost, bool IncludesCustomPeriod);