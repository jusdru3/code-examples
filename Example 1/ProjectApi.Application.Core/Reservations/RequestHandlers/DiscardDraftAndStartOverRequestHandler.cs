using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.RequestHandlers;

public class DiscardDraftAndStartOverRequestHandler : IDiscardDraftAndStartOverRequestHandler
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DiscardDraftAndStartOverRequestHandler(ApplicationDbContext applicationDbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _applicationDbContext = applicationDbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(Guid id)
    {
        var reservation = await _applicationDbContext.Reservations
            .Include(r => r.ReservationApproval)
            .FirstAsync(r => r.Id == id);

        reservation.ChangeToDiscarded();
        reservation.ReservationApproval?.Expire(_dateTimeProvider.NowUtc());

        await _applicationDbContext.SaveChangesAsync();
    }
}