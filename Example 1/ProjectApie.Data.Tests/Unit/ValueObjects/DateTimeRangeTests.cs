using ProjectUtilities.DateTime;
using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Data.Tests.Unit.ValueObjects;

public class DateTimeRangeTests
{
    [Fact]
    public void SingleDayRange()
    {
        var start = new DateTime(2023, 12, 29, 10, 30, 00);
        var end = new DateTime(2023, 12, 29, 15, 00, 00);
        var range = new DateTimeRange(start, end);

        var result = range.SplitIntoSingleDayRanges();

        Assert.Single(result);
        Assert.Equal(start, result[0].From);
        Assert.Equal(end, result[0].To);
    }

    [Fact]
    public void MultipleDayRange()
    {
        var start = new DateTime(2023, 12, 29, 23, 59, 00);
        var end = new DateTime(2024, 01, 01, 01, 01, 00);
        var range = new DateTimeRange(start, end);

        var result = range.SplitIntoSingleDayRanges();

        Assert.Equal(4, result.Count);
        Assert.Equal(start, result[0].From);
        Assert.Equal(new DateTime(2023, 12, 29, 23, 59, 59).ResetTimeToEndOfDay(), result[0].To);

        Assert.Equal(new DateTime(2023, 12, 30, 00, 00, 00), result[1].From);
        Assert.Equal(new DateTime(2023, 12, 30, 23, 59, 59).ResetTimeToEndOfDay(), result[1].To);

        Assert.Equal(new DateTime(2023, 12, 31, 00, 00, 00), result[2].From);
        Assert.Equal(new DateTime(2023, 12, 31, 23, 59, 59).ResetTimeToEndOfDay(), result[2].To);

        Assert.Equal(new DateTime(2024, 01, 01, 00, 00, 00), result[3].From);
        Assert.Equal(end, result[3].To);
    }

    [Fact]
    public void EdgeCaseAtMidnight()
    {
        var start = new DateTime(2023, 12, 29, 00, 00, 00);
        var end = new DateTime(2023, 12, 30, 00, 00, 00);
        var range = new DateTimeRange(start, end);

        var result = range.SplitIntoSingleDayRanges();

        Assert.Single(result);
        Assert.Equal(start, result[0].From);
        Assert.Equal(new DateTime(2023, 12, 29, 23, 59, 59).ResetTimeToEndOfDay(), result[0].To);
    }


    [Theory]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-05", "2024-03-07")]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-09", "2024-03-10")]
    [InlineData("2024-03-07T12:00:00", "2024-03-09", "2024-03-05", "2024-03-07T12:00:00")]
    [InlineData("2024-03-07T12:00:00", "2024-03-09", "2024-03-09", "2024-03-10T12:00:00")]
    public void FindNoOverlap(DateTime from1, DateTime to1, DateTime from2, DateTime to2)
    {
        var range1 = new DateTimeRange(from1, to1);
        var range2 = new DateTimeRange(from2, to2);

        var result = range1.GetOverlapOrDefault(range2);
        var invertedResult = range2.GetOverlapOrDefault(range1);

        Assert.Null(result);
        Assert.Null(invertedResult);
    }

    [Theory]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-05", "2024-03-08", "2024-03-07", "2024-03-08")]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-08", "2024-03-09", "2024-03-08", "2024-03-09")]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-07", "2024-03-09", "2024-03-07", "2024-03-09")]
    [InlineData("2024-03-07", "2024-03-09", "2024-03-08", "2024-03-10", "2024-03-08", "2024-03-09")]
    [InlineData("2024-03-07T12:00:00", "2024-03-09T12:00:00", "2024-03-08", "2024-03-10", "2024-03-08",
        "2024-03-09T12:00:00")]
    public void FindOverlap(DateTime from1, DateTime to1, DateTime from2, DateTime to2, DateTime resultFrom,
        DateTime resultTo)
    {
        var range1 = new DateTimeRange(from1, to1);
        var range2 = new DateTimeRange(from2, to2);
        var expectedRange = new DateTimeRange(resultFrom, resultTo);

        var result = range1.GetOverlapOrDefault(range2);
        var invertedResult = range2.GetOverlapOrDefault(range1);

        Assert.Equal(expectedRange, result);
        Assert.Equal(expectedRange, invertedResult);
    }
}