using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Application.Core.Reservations.Dtos;

public record ReservationBillingPeriodDto(
    Guid Id,
    Guid ReservationId,
    DateTimeRange DateTimeRangeInUtc,
    ReservationCostDto Cost
);