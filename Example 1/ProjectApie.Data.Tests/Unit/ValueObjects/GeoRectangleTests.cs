using ProjectApi.Data.Exceptions.ValueObjects;
using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Data.Tests.Unit.ValueObjects;

public class GeoRectangleTests
{
    [Fact]
    public void Constructor_ShouldCreateValidRectangle()
    {
        var northEastLatitude = new Latitude(10);
        var southWestLatitude = new Latitude(-10);
        var northEastLongitude = new Longitude(20);
        var southWestLongitude = new Longitude(-20);

        var rectangle = new GeoRectangle(northEastLatitude, northEastLongitude, southWestLatitude, southWestLongitude);

        Assert.Equal(northEastLatitude, rectangle.NorthEastLatitude);
        Assert.Equal(northEastLongitude, rectangle.NorthEastLongitude);
        Assert.Equal(southWestLatitude, rectangle.SouthWestLatitude);
        Assert.Equal(southWestLongitude, rectangle.SouthWestLongitude);
    }

    [Fact]
    public void Constructor_InvalidLatitudes_ShouldThrowException()
    {
        var invalidNorthEast = new Latitude(-10);
        var southWest = new Latitude(20);
        var east = new Longitude(20);
        var west = new Longitude(-20);

        var exception = Assert.Throws<GeoRectangleException>(() =>
            new GeoRectangle(invalidNorthEast, east, southWest, west));

        Assert.Equal(GeoRectangleException.InvalidLatitudes().Message, exception.Message);
    }

    [Fact]
    public void Constructor_InvalidLongitudes_ShouldThrowException()
    {
        var northEast = new Latitude(10);
        var southWest = new Latitude(-10);
        var invalidEast = new Longitude(-20);
        var west = new Longitude(20);

        var exception = Assert.Throws<GeoRectangleException>(() =>
            new GeoRectangle(northEast, invalidEast, southWest, west));

        Assert.Equal(GeoRectangleException.InvalidLongitudes().Message, exception.Message);
    }

    [Theory]
    [InlineData(0, 0, true)] // Point inside the rectangle
    [InlineData(20, 20, false)] // Point outside the rectangle
    [InlineData(-10, -20, true)] // Point at the corner of the rectangle
    public void Contains_ShouldReturnCorrectResult(double latitude, double longitude, bool expectedResult)
    {
        var northEast = new Latitude(10);
        var southWest = new Latitude(-10);
        var east = new Longitude(20);
        var west = new Longitude(-20);

        var rectangle = new GeoRectangle(northEast, east, southWest, west);
        var result = rectangle.Contains(new Latitude(latitude), new Longitude(longitude));

        Assert.Equal(expectedResult, result);
    }
}