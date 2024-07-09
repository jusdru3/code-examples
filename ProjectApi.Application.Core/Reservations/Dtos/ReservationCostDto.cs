namespace ProjectApi.Application.Core.Reservations.Dtos;

public record ReservationCostDto(
    decimal Amount,
    decimal ServiceFeePercentageInDecimal,
    decimal ServiceFee,
    decimal TotalAmount
);