using ProjectUtilities.Utilities.BackgroundJobs;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using Quartz;

namespace ProjectApi.BackgroundJobs;

public class ReservationLifecycleProgressionBackgroundJob : BackgroundJob
{
    private readonly IReservationsLifecyclesService _reservationsLifecyclesService;
    private readonly IReservationsChargingService _reservationsChargingService;

    public ReservationLifecycleProgressionBackgroundJob(ILogger<ReservationLifecycleProgressionBackgroundJob> logger,
        IReservationsLifecyclesService reservationsLifecyclesService,
        IReservationsChargingService reservationsChargingService)
        : base(logger)
    {
        _reservationsLifecyclesService = reservationsLifecyclesService;
        _reservationsChargingService = reservationsChargingService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        await _reservationsLifecyclesService.StartReservations();
        await _reservationsLifecyclesService.CompleteReservations();
        await _reservationsLifecyclesService.TerminateReservations();

        await _reservationsChargingService.ChargeDueReservations();
        await _reservationsChargingService.RetryPendingReservations();
    }

    public override ITrigger Trigger()
    {
        var triggerKey = $"{GetType()}Trigger";
        return TriggerBuilder.Create()
            .ForJob(JobKey)
            .WithIdentity(triggerKey)
            .WithSimpleSchedule(s => s.RepeatForever().WithInterval(TimeSpan.FromMinutes(1)))
            .Build();
    }
}