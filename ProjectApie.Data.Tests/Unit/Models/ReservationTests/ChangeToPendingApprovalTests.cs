using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.ValueObjects;
using ProjectApi.Tests.Utilities.Builders.Reservations;

namespace ProjectApi.Data.Tests.Unit.Models.ReservationTests;

public class ChangeToPendingApprovalTests
{
    [Fact]
    public void ReservationStatusChangesToPendingApproval()
    {
        var localTimezone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Draft)
            .Build();

        reservation.ChangeToPendingApproval(DateTime.Now.ToUniversalTime(), localTimezone);

        Assert.Equal(ReservationStatus.PendingApproval, reservation.Status);
    }

    [Fact]
    public void ReservationApprovalExpirySet24HFromNow()
    {
        var now = DateTime.Parse("2024-01-09T12:00:00Z").ToUniversalTime();
        var reservationStartDateInUtc = DateTime.Parse("2024-01-12T12:00:00Z");
        var localTimezone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Draft)
            .WithDateTimeRangeInUtc(new DateTimeRange(reservationStartDateInUtc,
                reservationStartDateInUtc.AddHours(1)))
            .Build();

        reservation.ChangeToPendingApproval(now, localTimezone);

        Assert.Equal(ReservationStatus.PendingApproval, reservation.Status);
        Assert.Equal(now.Add(TimeSpan.FromDays(1)), reservation.ReservationApproval?.ExpireAt);
    }


    [Fact]
    public void ReservationApprovalExpirySetToReservationStart()
    {
        var now = DateTime.Parse("2024-01-09T12:00:00Z").ToUniversalTime();
        var reservationStartDateInUtc = DateTime.Parse("2024-01-09T17:00:00Z").ToUniversalTime();
        var localTimezone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Draft)
            .WithDateTimeRangeInUtc(new DateTimeRange(reservationStartDateInUtc,
                reservationStartDateInUtc.AddHours(1)))
            .Build();

        reservation.ChangeToPendingApproval(now, localTimezone);

        Assert.Equal(ReservationStatus.PendingApproval, reservation.Status);
        Assert.Equal(reservationStartDateInUtc, reservation.ReservationApproval?.ExpireAt);
    }
}