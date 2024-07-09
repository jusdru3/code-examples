using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Application.Core.Reservations.Services.Interfaces;

public interface IReservationCostsService
{
    Task<GetReservationCostResponseDto> GetReservationCost(Guid spotId, DateTimeRange dateTimeRange, Guid? boatId, bool includeElectricity);
}