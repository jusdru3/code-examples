using AutoMapper;
using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Application.Core.Users.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.RequestHandlers;

public class CancelReservationRequestHandler : ICancelReservationRequestHandler
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ICurrentApplicationUserStore _currentApplicationUserStore;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMapper _mapper;

    public CancelReservationRequestHandler(ApplicationDbContext applicationDbContext,
        ICurrentApplicationUserStore currentApplicationUserStore, IDateTimeProvider dateTimeProvider, IMapper mapper)
    {
        _applicationDbContext = applicationDbContext;
        _currentApplicationUserStore = currentApplicationUserStore;
        _dateTimeProvider = dateTimeProvider;
        _mapper = mapper;
    }

    public async Task<ReservationCancelationDto> Handle(Guid id)
    {
        var currentUser = await _currentApplicationUserStore.GetCurrentApplicationUser();

        var reservation = await _applicationDbContext.Reservations
            .ByUserId(currentUser.Id)
            .WithIncludes()
            .Where(e => e.Status == ReservationStatus.Confirmed)
            .FirstAsync(e => e.Id == id);

        reservation.ChangeToCanceled(currentUser.Id, _dateTimeProvider.NowUtc());

        await _applicationDbContext.SaveChangesAsync();

        return _mapper.Map<ReservationCancelationDto>(reservation.Cancelation);
    }
}