namespace ProjectApi.Application.Core.Reservations.Services.Interfaces;

public interface IReservationsCleanupService
{
    Task ExpireDrafts();
    Task ExpireApprovals();
}