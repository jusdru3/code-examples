namespace ProjectApi.Application.Core.Reservations.Services.Interfaces;

public interface IReservationsLifecyclesService
{
    Task CompleteReservations();
    Task StartReservations();
    Task TerminateReservations();
}