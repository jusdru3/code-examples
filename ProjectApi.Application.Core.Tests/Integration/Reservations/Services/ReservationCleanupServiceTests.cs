using ProjectUtilities.DateTime.Services.Interfaces;
using Bogus;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Application.Core.Tests.Utilities.Fixtures;
using ProjectApi.Application.Core.Users.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Tests.Utilities.Builders.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ProjectApi.Application.Core.Tests.Integration.Reservations.Services;

public class ReservationCleanupServiceTests : IClassFixture<BaseApplicationFixture>, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IReservationsCleanupService _testingInstance;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ICurrentApplicationUserStore> _currentApplicationUserStoreMock = new();

    public ReservationCleanupServiceTests(BaseApplicationFixture baseFixture)
    {
        _serviceProvider = baseFixture.BuildAndGetServiceProvider(collection =>
        {
            collection.AddSingleton(_dateTimeProvider.Object);
            collection.AddSingleton(_currentApplicationUserStoreMock.Object);
        });

        _applicationDbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _testingInstance = _serviceProvider.GetRequiredService<IReservationsCleanupService>();

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
    [InlineData("2024-01-01T12:00:00Z")]
    [InlineData("2024-01-01T11:59:59Z")]
    public async Task MarkDraftsAsTimeout(DateTime reservationDraftExpirationDate)
    {
        var universalTime = DateTime.Parse("2024-01-01T12:00:00Z").ToUniversalTime();
        _dateTimeProvider.Setup(e => e.NowUtc()).Returns(universalTime);
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Draft)
            .WithDraftExpirationTime(reservationDraftExpirationDate.ToUniversalTime())
            .Build();
        await _applicationDbContext.AddAsync(reservation);
        await _applicationDbContext.SaveChangesAsync();

        await _testingInstance.ExpireDrafts();

        var resultReservation = await _applicationDbContext.Reservations.FirstAsync(e => e.Id == reservation.Id);
        Assert.Equal(ReservationStatus.PaymentTimedOut, resultReservation.Status);
    }

    [Theory]
    [InlineData("2024-01-01T12:00:01Z")]
    [InlineData("2024-01-01T12:46:00Z")]
    public async Task ReservationDraftExpirationInFuture(DateTime reservationDraftExpirationDate)
    {
        var universalTime = DateTime.Parse("2024-01-01T12:00:00Z").ToUniversalTime();
        _dateTimeProvider.Setup(e => e.NowUtc()).Returns(universalTime);
        var reservation = new ReservationBuilder()
            .WithStatus(ReservationStatus.Draft)
            .WithDraftExpirationTime(reservationDraftExpirationDate.ToUniversalTime())
            .Build();
        await _applicationDbContext.AddAsync(reservation);
        await _applicationDbContext.SaveChangesAsync();

        await _testingInstance.ExpireDrafts();

        var resultReservation = await _applicationDbContext.Reservations.FirstAsync(e => e.Id == reservation.Id);
        Assert.Equal(ReservationStatus.Draft, resultReservation.Status);
    }

    [Fact]
    public async Task ReservationIsNotDraft()
    {
        var status = new Faker().PickRandom(ReservationStatus.List.Except(new[] { ReservationStatus.Draft }));
        var universalTime = DateTime.Parse("2024-01-01T12:00:00Z").ToUniversalTime();
        _dateTimeProvider.Setup(e => e.NowUtc()).Returns(universalTime);
        var reservation = new ReservationBuilder()
            .WithStatus(status)
            .WithDraftExpirationTime(DateTime.Parse("2024-01-01T10:00:00Z").ToUniversalTime())
            .Build();
        await _applicationDbContext.AddAsync(reservation);
        await _applicationDbContext.SaveChangesAsync();

        await _testingInstance.ExpireDrafts();

        var resultReservation = await _applicationDbContext.Reservations.FirstAsync(e => e.Id == reservation.Id);
        Assert.NotEqual(ReservationStatus.Draft, resultReservation.Status);
        Assert.Equal(status, resultReservation.Status);
    }

    [Fact]
    public async Task ApprovalExpireDateTooRecent()
    {
        var universalTime = DateTime.Parse("2024-01-04T12:00:00Z").ToUniversalTime();
        _dateTimeProvider.Setup(e => e.NowUtc()).Returns(universalTime);

        var approval = new ReservationApprovalBuilder()
            .WithExpireAt(DateTime.Parse("2024-01-04T12:00:01Z").ToUniversalTime())
            .Build();
        await _applicationDbContext.AddAsync(approval);
        await _applicationDbContext.SaveChangesAsync();

        await _testingInstance.ExpireApprovals();

        var resultApproval = await _applicationDbContext.ReservationApprovals.FirstAsync(e => e.Id == approval.Id);
        Assert.Null(resultApproval.Status);
    }

    [Fact]
    public async Task ApprovalShouldBecomeExpired()
    {
        var universalTime = DateTime.Parse("2024-01-04T12:00:00Z").ToUniversalTime();
        _dateTimeProvider.Setup(e => e.NowUtc()).Returns(universalTime);

        var approval = new ReservationApprovalBuilder()
            .WithExpireAt(DateTime.Parse("2024-01-04T12:00:00Z").ToUniversalTime())
            .WithReservation(new ReservationBuilder()
                .WithStatus(ReservationStatus.PendingApproval)
                .Build()
            )
            .Build();
        await _applicationDbContext.AddAsync(approval);
        await _applicationDbContext.SaveChangesAsync();

        await _testingInstance.ExpireApprovals();

        var resultApproval = await _applicationDbContext.ReservationApprovals.FirstAsync(e => e.Id == approval.Id);
        Assert.Equal(ReservationApprovalStatus.Expired, resultApproval.Status);
    }
}