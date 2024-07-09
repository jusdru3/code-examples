using AutoMapper;
using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Reservations.OwnedTypes;

namespace ProjectApi.Application.Core.Reservations.AutoMapperProfiles;

public class ReservationProfile : Profile
{
    public ReservationProfile()
    {
        CreateMap<Reservation, ReservationDto>();
        CreateMap<ReservationCost, ReservationCostDto>();
        CreateMap<ReservationCancelation, ReservationCancelationDto>();
        CreateMap<ReservationBillingPeriod, ReservationBillingPeriodDto>();
        CreateMap<ReservationTermination, ReservationTerminationDto>();
    }
}