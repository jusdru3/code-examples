using ProjectApi.Data.Exceptions;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.ValueObjects;
using ProjectApi.Tests.Utilities.Builders.Reservations;

namespace ProjectApi.Data.Tests.Unit.Models.ReservationTests;

public class ChangeToCanceledTests
{
    [Theory]
    [MemberData(nameof(WrongStatusMemberData))]
    public void WrongStatus(ReservationStatus status)
    {
        var reservation = new ReservationBuilder()
            .WithStatus(status)
            .Build();

        var exception = Assert.Throws<ReservationException>(() =>
        {
            reservation.ChangeToCanceled(Guid.NewGuid(), DateTime.UtcNow);
        });
        Assert.Equal(ReservationException.InvalidStatusChange(status, ReservationStatus.Canceled).Message,
            exception.Message);
    }

    public static IEnumerable<object[]> WrongStatusMemberData()
    {
        return ReservationStatus.List
            .Where(status => status != ReservationStatus.Confirmed)
            .Select(status => new object[] { status });
    }

    [Fact]
    public void CanceledBySpotOwnerFullRefundAmount()
    {
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Confirmed)
            .WithBillingPeriods(new ReservationBillingPeriodBuilder()
                .WithPaymentStatus(ReservationBillingPeriodPaymentStatus.Paid)
                .Build()
            )
            .Build();
        var billingPeriod = reservation.BillingPeriods!.First();
        var spotOwner = reservation.Spot!.ApplicationUser!;

        reservation.ChangeToCanceled(spotOwner.Id, DateTime.UtcNow);

        Assert.Equal(ReservationStatus.Canceled, reservation.Status);
        Assert.Equal(billingPeriod.Cost.Amount, reservation.Cancelation!.RefundAmount);
    }

    [Theory]
    [InlineData("2024-01-15T12:00:00Z")]
    [InlineData("2024-01-15T11:59:59Z")]
    [InlineData("2024-01-15T05:59:59Z")]
    [InlineData("2024-01-14T12:00:00Z")]
    public void CanceledByUserHalfAmountRefund(DateTime localCurrentTimeInUtc)
    {
        var reservationStartDateInUtc = DateTime.Parse("2024-01-16T12:00:00Z");

        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Confirmed)
            .WithDateTimeRangeInUtc(new DateTimeRange(reservationStartDateInUtc,
                reservationStartDateInUtc.AddHours(1)))
            .WithBillingPeriods(new ReservationBillingPeriodBuilder()
                .WithPaymentStatus(ReservationBillingPeriodPaymentStatus.Paid)
                .Build()
            )
            .Build();
        var reservationUser = reservation.ApplicationUser!;
        var billingPeriod = reservation.BillingPeriods!.First();

        reservation.ChangeToCanceled(reservationUser.Id, localCurrentTimeInUtc);

        Assert.Equal(ReservationStatus.Canceled, reservation.Status);
        Assert.Equal(billingPeriod.Cost.Amount / 2, reservation.Cancelation!.RefundAmount);
    }

    [Theory]
    [InlineData("2024-01-16T12:00:00Z")]
    [InlineData("2024-01-16T11:59:59Z")]
    [InlineData("2024-01-16T06:00:00Z")]
    [InlineData("2024-01-15T12:00:01Z")]
    public void CanceledByUserZeroAmountRefund(DateTime localCurrentTimeInUtc)
    {
        var reservationStartDateInUtc = DateTime.Parse("2024-01-16T12:00:00");

        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Confirmed)
            .WithBillingPeriods(new ReservationBillingPeriodBuilder()
                .WithPaymentStatus(ReservationBillingPeriodPaymentStatus.Paid)
                .Build()
            )
            .WithDateTimeRangeInUtc(new DateTimeRange(reservationStartDateInUtc,
                reservationStartDateInUtc.AddHours(1)))
            .Build();
        var reservationUser = reservation.ApplicationUser!;

        reservation.ChangeToCanceled(reservationUser.Id, DateTime.UtcNow);

        Assert.Equal(ReservationStatus.Canceled, reservation.Status);
        Assert.Equal(0, reservation.Cancelation!.RefundAmount);
    }
}