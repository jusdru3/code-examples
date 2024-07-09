namespace ProjectApi.Application.Core.Reservations.Services.Interfaces;

public interface IReservationsChargingService
{
    Task ChargeDueReservations();
    Task RetryPendingReservations();
}