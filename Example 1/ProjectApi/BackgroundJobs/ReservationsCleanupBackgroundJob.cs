using ProjectUtilities.Utilities.BackgroundJobs;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using Quartz;

namespace ProjectApi.BackgroundJobs;

public class ReservationsCleanupBackgroundJob : BackgroundJob
{
    private readonly IReservationsCleanupService _reservationsCleanupService;

    public ReservationsCleanupBackgroundJob(ILogger<ReservationsCleanupBackgroundJob> logger,
        IReservationsCleanupService reservationsCleanupService, JobKey? jobKey = null) : base(logger, jobKey)
    {
        _reservationsCleanupService = reservationsCleanupService;
    }

    protected override async Task ExecuteJob(IJobExecutionContext context)
    {
        await _reservationsCleanupService.ExpireDrafts();
        await _reservationsCleanupService.ExpireApprovals();
    }

    public override ITrigger Trigger()
    {
        var triggerKey = new TriggerKey($"{GetType()}Trigger");
        return TriggerBuilder.Create()
            .ForJob(JobKey)
            .WithCronSchedule("0 * * * * ?")
            .WithIdentity(triggerKey)
            .Build();
    }
}