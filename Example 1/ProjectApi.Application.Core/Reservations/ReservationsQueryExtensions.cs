using ProjectApi.Data.Models.Reservations;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations;

public static class ReservationsQueryExtensions
{
    public static IQueryable<Reservation> ByUserId(this IQueryable<Reservation> query, Guid applicationUserId)
    {
        return query.Where(e => e.ApplicationUserId == applicationUserId);
    }

    public static IQueryable<Reservation> WithIncludes(this IQueryable<Reservation> query)
    {
        return query.Include(e => e.Boat)
            .Include(e => e.Spot)
            .ThenInclude(e => e.Amenities)
            .Include(e => e.Spot)
            .ThenInclude(e => e.SpotPhotos)
            .Include(e => e.BillingPeriods)
            .Include(e => e.Cancelation);
    }
}