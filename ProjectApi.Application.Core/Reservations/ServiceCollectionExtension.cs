using ProjectApi.Application.Core.Reservations.RequestHandlers;
using ProjectApi.Application.Core.Reservations.RequestHandlers.Interfaces;
using ProjectApi.Application.Core.Reservations.Services;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectApi.Application.Core.Reservations;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddReservationsServices(this IServiceCollection serviceCollection)
    {
        RegisterRequestHandlers(serviceCollection);

        RegisterServices(serviceCollection);

        return serviceCollection;
    }

    private static void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IReservationsLifecyclesService, ReservationsLifecyclesService>();
        serviceCollection.AddScoped<IReservationsCleanupService, ReservationsCleanupService>();
        serviceCollection.AddScoped<IReservationCostsService, ReservationCostsService>();
        serviceCollection.AddScoped<IReservationsChargingService, ReservationsChargingService>();
    }

    private static void RegisterRequestHandlers(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ICreateReservationRequestHandler, CreateReservationRequestHandler>();
        serviceCollection.AddScoped<IReadReservationRequestHandler, ReadReservationRequestHandler>();
        serviceCollection.AddScoped<IReadByIdReservationRequestHandler, ReadByIdReservationRequestHandler>();
        serviceCollection.AddScoped<IDeleteReservationRequestHandler, DeleteReservationRequestHandler>();
        serviceCollection.AddScoped<IWithdrawRequestHandler, WithdrawRequestHandler>();
        serviceCollection.AddScoped<IPayReservationRequestHandler, PayReservationRequestHandler>();
        serviceCollection.AddScoped<ICancelReservationRequestHandler, CancelReservationRequestHandler>();
        serviceCollection.AddScoped<IDiscardDraftAndStartOverRequestHandler, DiscardDraftAndStartOverRequestHandler>();
    }
}