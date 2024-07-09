using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Application.Core.Users.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.RequestHandlers;

public class DeleteReservationRequestHandler : IDeleteReservationRequestHandler
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ICurrentApplicationUserStore _currentApplicationUserStore;

    public DeleteReservationRequestHandler(ApplicationDbContext applicationDbContext,
        ICurrentApplicationUserStore currentApplicationUserStore)
    {
        _applicationDbContext = applicationDbContext;
        _currentApplicationUserStore = currentApplicationUserStore;
    }

    public async Task Handle(Guid id)
    {
        var currentUser = await _currentApplicationUserStore.GetCurrentApplicationUser();

        var entity = await _applicationDbContext.Reservations
            .ByUserId(currentUser.Id)
            .FirstAsync(e => e.Id == id);

        if (!entity.IsDeletable()) throw EntityException.CantDelete();

        _applicationDbContext.Remove(entity);
        await _applicationDbContext.SaveChangesAsync();
    }
}