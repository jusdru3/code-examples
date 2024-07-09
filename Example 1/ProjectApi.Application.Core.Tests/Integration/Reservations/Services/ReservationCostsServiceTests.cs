using ProjectUtilities.DateTime.Services.Interfaces;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Application.Core.Tests.Utilities.Fixtures;
using ProjectApi.Application.Core.Users.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Spots;
using ProjectApi.Data.ValueObjects;
using ProjectApi.Tests.Utilities.Builders;
using ProjectApi.Tests.Utilities.Builders.Spots;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ProjectApi.Application.Core.Tests.Integration.Reservations.Services;

public class ReservationCostsServiceTests : IClassFixture<BaseApplicationFixture>, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IReservationCostsService _testingInstance;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ICurrentApplicationUserStore> _currentApplicationUserStoreMock = new();

    public ReservationCostsServiceTests(BaseApplicationFixture baseFixture)
    {
        _serviceProvider = baseFixture.BuildAndGetServiceProvider(collection =>
        {
            collection.AddSingleton(_dateTimeProvider.Object);
            collection.AddSingleton(_currentApplicationUserStoreMock.Object);
        });

        _applicationDbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _testingInstance = _serviceProvider.GetRequiredService<IReservationCostsService>();

        _applicationDbContext.Database.BeginTransaction();
    }

    public void Dispose()
    {
        _applicationDbContext.Database.RollbackTransaction();
    }

    [Fact]
    public void InstanceRegistered()
    {
        Assert.NotNull(_testingInstance);
    }

    [Theory]
    [InlineData("2024-01-01T12:00:00", "2024-01-01T12:30:00", 1, 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-01T13:00:01", 1, 2 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T11:00:00", 1, 23 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T11:59:59", 1, 24 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-01T12:30:00", 5, 5 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-01T13:00:01", 5, 5 * 2 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T11:00:00", 5, 5 * 23 * 10)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T11:59:59", 5, 5 * 24 * 10)]
    public async Task CalculateTotalByHourlyPricings(DateTime from, DateTime to, double length,
        decimal totalPriceWithoutCommissions)
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(10)
            .WithPriceDaily(100)
            .WithPriceWeekly(500)
            .WithPriceMonthly(1800)
            .WithMinimumLengthForPricing(1)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(length)
            .Build();
        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(spot);
        await _applicationDbContext.SaveChangesAsync();
        var range = new DateTimeRange(from, to);

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(totalPriceWithoutCommissions), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T12:00:00", 1, 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T18:00:00", 1, 1.25 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-03T00:00:00", 1, 1.5 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-03T12:00:00", 1, 2 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T12:00:00", 5, 5 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-02T18:00:00", 5, 5 * 1.25 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-03T00:00:00", 5, 5 * 1.5 * 100)]
    [InlineData("2024-01-01T12:00:00", "2024-01-03T12:00:00", 5, 5 * 2 * 100)]
    public async Task CalculateTotalByDailyPricings(DateTime from, DateTime to, double length,
        decimal totalPriceWithoutCommissions)
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(10)
            .WithPriceDaily(100)
            .WithPriceWeekly(500)
            .WithPriceMonthly(1800)
            .WithMinimumLengthForPricing(1)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(length)
            .Build();
        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(spot);
        await _applicationDbContext.SaveChangesAsync();
        var range = new DateTimeRange(from, to);

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(totalPriceWithoutCommissions), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-03-07T12:00:00", "2024-03-14T12:00:00", 1, 500)]
    [InlineData("2024-03-07T12:00:00", "2024-03-14T12:00:00", 5, 5 * 500)]
    [InlineData("2024-03-07T12:00:00", "2024-03-18T00:00:00", 1, 1.5 * 500)]
    [InlineData("2024-03-07T12:00:00", "2024-03-18T00:00:00", 5, 5 * 1.5 * 500)]
    [InlineData("2024-03-07T12:00:00", "2024-03-21T12:00:00", 1, 2 * 500)]
    [InlineData("2024-03-07T12:00:00", "2024-03-21T12:00:00", 5, 5 * 2 * 500)]
    public async Task CalculateTotalByWeeklyPricings(DateTime from, DateTime to, double length,
        decimal totalPriceWithoutCommissions)
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(10)
            .WithPriceDaily(100)
            .WithPriceWeekly(500)
            .WithPriceMonthly(1800)
            .WithMinimumLengthForPricing(1)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(length)
            .Build();
        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(spot);
        await _applicationDbContext.SaveChangesAsync();
        var range = new DateTimeRange(from, to);

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(totalPriceWithoutCommissions), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-03-01T12:00:00", "2024-03-31T12:00:00", 1, 1800)]
    [InlineData("2024-03-01T12:00:00", "2024-03-31T12:00:00", 5, 5 * 1800)]
    [InlineData("2024-03-01T12:00:00", "2024-04-15T12:00:00", 1, 1.5 * 1800)]
    [InlineData("2024-03-01T12:00:00", "2024-04-15T12:00:00", 5, 5 * 1.5 * 1800)]
    [InlineData("2024-03-01T12:00:00", "2024-04-30T12:00:00", 1, 2 * 1800)]
    [InlineData("2024-03-01T12:00:00", "2024-04-30T12:00:00", 5, 5 * 2 * 1800)]
    public async Task CalculateTotalByMonthlyPricings(DateTime from, DateTime to, double length,
        decimal totalPriceWithoutCommissions)
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(10)
            .WithPriceDaily(100)
            .WithPriceWeekly(500)
            .WithPriceMonthly(1800)
            .WithMinimumLengthForPricing(1)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(length)
            .Build();
        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(spot);
        await _applicationDbContext.SaveChangesAsync();
        var range = new DateTimeRange(from, to);

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(totalPriceWithoutCommissions), result.ReservationCost.Amount);
    }

    [Fact]
    public async Task CalculateTotalPriceByMinimumLengthForPricing()
    {
        var length = 5;
        var spot = new SpotBuilder()
            .WithPriceHourly(10)
            .WithMinimumLengthForPricing(10)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(length)
            .Build();
        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(spot);
        await _applicationDbContext.SaveChangesAsync();
        var range = new DateTimeRange(DateTime.Parse("2024-03-01T12:00:00"), DateTime.Parse("2024-03-01T13:00:00"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(10 * 10), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-02-01", "2024-02-29T23:59:59.999", 10 * 100)]
    [InlineData("2024-02-01", "2024-03-01", 10 * 100)]
    [InlineData("2024-02-01", "2024-03-16", 10 * (0.5 * 100 + 0.5 * 200))]
    [InlineData("2024-03-10", "2024-03-25", 10 * (0.5 * 100 + 0.5 * 200))]
    [InlineData("2024-03-15", "2024-03-30", 10 * (0.5 * 100 + 0.5 * 200))]
    [InlineData("2024-03-25", "2024-04-30", 10 * (0.8 * 100 + 0.2 * 200))]
    [InlineData("2024-04-01", "2024-04-30", 10 * 100)]
    public async Task CalculateTotalPriceByMonthlyWithCustomPricing(DateTime customPricingFrom,
        DateTime customPricingTo, decimal total)
    {
        var spot = new SpotBuilder()
            .WithPriceMonthly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(customPricingFrom, customPricingTo))
            .WithPriceMonthly(200)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-31"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Fact]
    public async Task CalculateTotalPriceByMonthlyWithMultipleCustomPricings()
    {
        var spot = new SpotBuilder()
            .WithPriceMonthly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing1 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-07")))
            .WithPriceMonthly(200)
            .WithSpot(spot)
            .Build();
        var customPricing2 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-16"), DateTime.Parse("2024-03-22")))
            .WithPriceMonthly(400)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing1);
        await _applicationDbContext.AddAsync(customPricing2);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-31"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        var total = (decimal)(10 * (0.6 * 100 + 0.2 * 200 + 0.2 * 400));
        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-02-01", "2024-02-29T23:59:59.999", 2 * 10 * 100)]
    [InlineData("2024-02-01", "2024-03-01", 10 * 2 * 100)]
    [InlineData("2024-02-20", "2024-03-08", 10 * (100 + 200))]
    [InlineData("2024-03-01", "2024-03-08", 10 * (100 + 200))]
    [InlineData("2024-03-05", "2024-03-12", 10 * (100 + 200))]
    [InlineData("2024-03-08", "2024-03-30", 10 * (100 + 200))]
    [InlineData("2024-03-15", "2024-03-30", 10 * 2 * 100)]
    public async Task CalculateTotalPriceByWeeklyWithCustomPricing(DateTime customPricingFrom,
        DateTime customPricingTo, decimal total)
    {
        var spot = new SpotBuilder()
            .WithPriceWeekly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(customPricingFrom, customPricingTo))
            .WithPriceWeekly(200)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-15"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Fact]
    public async Task CalculateTotalPriceByWeeklyWithMultipleCustomPricings()
    {
        var spot = new SpotBuilder()
            .WithPriceWeekly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing1 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-04T12:00:00")))
            .WithPriceWeekly(200)
            .WithSpot(spot)
            .Build();
        var customPricing2 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-11T12:00:00"), DateTime.Parse("2024-03-15")))
            .WithPriceWeekly(400)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing1);
        await _applicationDbContext.AddAsync(customPricing2);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-15"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        var total = (decimal)(10 * (100 + 0.5 * 200 + 0.5 * 400));
        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-02-01", "2024-02-29T23:59:59.999", 10 * 5 * 100)]
    [InlineData("2024-02-01", "2024-03-03", 10 * (3 * 100 + 2 * 200))]
    [InlineData("2024-03-02", "2024-03-04", 10 * (3 * 100 + 2 * 200))]
    [InlineData("2024-03-02", "2024-03-10", 10 * (1 * 100 + 4 * 200))]
    [InlineData("2024-03-06", "2024-03-10", 10 * (5 * 100))]
    public async Task CalculateTotalPriceByDailyWithCustomPricing(DateTime customPricingFrom,
        DateTime customPricingTo, decimal total)
    {
        var spot = new SpotBuilder()
            .WithPriceDaily(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(customPricingFrom, customPricingTo))
            .WithPriceDaily(200)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-06"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }


    [Fact]
    public async Task CalculateTotalPriceByDailyWithMultipleCustomPricings()
    {
        var spot = new SpotBuilder()
            .WithPriceDaily(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing1 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-03")))
            .WithPriceDaily(200)
            .WithSpot(spot)
            .Build();
        var customPricing2 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-05")))
            .WithPriceDaily(400)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing1);
        await _applicationDbContext.AddAsync(customPricing2);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-07"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        var total = (decimal)(10 * (2 * 100 + 2 * 200 + 2 * 400));
        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Theory]
    [InlineData("2024-02-01", "2024-02-29T23:59:59.999", 10 * 12 * 100)]
    [InlineData("2024-02-01", "2024-03-01T06:00:00", 10 * (6 * 100 + 6 * 200))]
    [InlineData("2024-03-01T06:00:00", "2024-03-01T18:00:00", 10 * (6 * 100 + 6 * 200))]
    [InlineData("2024-03-01T18:00:00", "2024-03-20", 10 * 12 * 100)]
    public async Task CalculateTotalPriceByHourlyWithCustomPricing(DateTime customPricingFrom,
        DateTime customPricingTo, decimal total)
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(customPricingFrom, customPricingTo))
            .WithPriceHourly(200)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-01T12:00:00"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }

    [Fact]
    public async Task CalculateTotalHourlyByDailyWithMultipleCustomPricings()
    {
        var spot = new SpotBuilder()
            .WithPriceHourly(100)
            .WithMinimumLengthForPricing(10)
            .Build();
        var customPricing1 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-01T03:00:00")))
            .WithPriceHourly(200)
            .WithSpot(spot)
            .Build();
        var customPricing2 = new SpotCustomPricingBuilder()
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-03"), DateTime.Parse("2024-03-05")))
            .WithEffectivePeriod(new DateTimeRange(DateTime.Parse("2024-03-01T03:00:00"),
                DateTime.Parse("2024-03-01T06:00:00")))
            .WithPriceHourly(400)
            .WithSpot(spot)
            .Build();
        var boat = new BoatBuilder()
            .WithLength(1)
            .Build();

        await _applicationDbContext.AddAsync(boat);
        await _applicationDbContext.AddAsync(customPricing1);
        await _applicationDbContext.AddAsync(customPricing2);
        await _applicationDbContext.SaveChangesAsync();

        var range = new DateTimeRange(DateTime.Parse("2024-03-01"), DateTime.Parse("2024-03-01T12:00:00"));

        var result = await _testingInstance.GetReservationCost(spot.Id, range, boat.Id, false);

        var total = (decimal)(10 * (6 * 100 + 3 * 200 + 3 * 400));
        Assert.Equal(Spot.AddCommissionsToPrice(total), result.ReservationCost.Amount);
    }
}