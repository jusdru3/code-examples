using ProjectUtilities.DateTime;
using ProjectApi.Application.Core.MySpotCustomPricings;
using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Application.Core.Reservations.Services.Interfaces;
using ProjectApi.Data.Contexts;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Spots;
using ProjectApi.Data.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ProjectApi.Application.Core.Reservations.Services;

public class ReservationCostsService : IReservationCostsService
{
    private readonly ApplicationDbContext _applicationDbContext;

    public ReservationCostsService(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<GetReservationCostResponseDto> GetReservationCost(Guid spotId, DateTimeRange dateTimeRange,
        Guid? boatId, bool includeElectricity)
    {
        var spot = await _applicationDbContext.Spots.FirstAsync(e => e.Id == spotId);
        var boat = await _applicationDbContext.Boats.FirstOrDefaultAsync(e => e.Id == boatId);
        var spotCustomPricings = await _applicationDbContext.SpotCustomPricings
            .Where(e => e.SpotId == spotId)
            .ByEffectivePeriodRange(dateTimeRange)
            .ToListAsync();

        if (boat == null)
        {
            return new GetReservationCostResponseDto(null, spotCustomPricings.Any());
        }

        var difference = dateTimeRange.GetDifference();
        var differenceInDays = difference.TotalDays;
        var differenceInWeeks = difference.GetTotalWeeks();
        var differenceInMonths = difference.GetTotalMonths();
        var differenceInHours = difference.TotalHours;

        var chargeableBoatLength = boat.Length < spot.MinimumLengthForPricing
            ? (decimal)spot.MinimumLengthForPricing
            : (decimal)boat.Length;

        if (differenceInMonths >= 1)
        {
            return new GetReservationCostResponseDto(CalculateMonthlyReservationCost(dateTimeRange, spotCustomPricings,
                differenceInMonths, spot, chargeableBoatLength, includeElectricity), spotCustomPricings.Any());
        }

        if (differenceInWeeks >= 1)
        {
            return new GetReservationCostResponseDto(CalculateWeeklyReservationCost(dateTimeRange, spotCustomPricings,
                differenceInWeeks, spot, chargeableBoatLength, includeElectricity), spotCustomPricings.Any());
        }

        if (differenceInDays >= 1)
        {
            return new GetReservationCostResponseDto(CalculateDailyReservationCost(dateTimeRange, spotCustomPricings,
                differenceInDays, spot, chargeableBoatLength, includeElectricity), spotCustomPricings.Any());
        }

        return new GetReservationCostResponseDto(CalculateHourlyReservationCost(dateTimeRange, differenceInHours,
            spotCustomPricings, spot, chargeableBoatLength, includeElectricity), spotCustomPricings.Any());
    }

    private static ReservationCost CalculateHourlyReservationCost(DateTimeRange dateTimeRange, double differenceInHours,
        List<SpotCustomPricing> spotCustomPricings, Spot spot, decimal chargeableBoatLength, bool includeElectricity)
    {
        var differenceInHoursCeiling = Math.Ceiling(differenceInHours);
        var totalAmountWithCommissionsForDaily = 0m;
        var electricityPrice = 0m;
        foreach (var spotCustomPricing in spotCustomPricings)
        {
            var effectivePeriodFromPricing = spotCustomPricing.EffectivePeriod.GetOverlapOrDefault(dateTimeRange);
            if (effectivePeriodFromPricing is null) continue;

            var effectivePeriodFromPricingInHours = effectivePeriodFromPricing.GetDifference().TotalHours;
            differenceInHoursCeiling -= effectivePeriodFromPricingInHours;

            totalAmountWithCommissionsForDaily
                += spotCustomPricing.PriceHourly.PricePerFootWithCommissions *
                   (decimal)effectivePeriodFromPricingInHours;
        }

        if (differenceInHoursCeiling > 0)
        {
            totalAmountWithCommissionsForDaily
                +=
                (decimal)differenceInHoursCeiling * spot.PriceHourly.PricePerFootWithCommissions;
        }

        if (includeElectricity && spot.Electricity is not null)
        {
            electricityPrice = (decimal)dateTimeRange.GetDifference().TotalHours * spot.Electricity.HourlyPrice;
        }

        return new ReservationCost(totalAmountWithCommissionsForDaily * chargeableBoatLength, electricityPrice);
    }

    private static ReservationCost CalculateDailyReservationCost(DateTimeRange dateTimeRange,
        List<SpotCustomPricing> spotCustomPricings,
        double differenceInDays, Spot spot, decimal chargeableBoatLength, bool includeElectricity)
    {
        var totalAmountWithCommissions = 0m;
        var electricityPrice = 0m;
        foreach (var spotCustomPricing in spotCustomPricings)
        {
            var effectivePeriodFromPricing = spotCustomPricing.EffectivePeriod.GetOverlapOrDefault(dateTimeRange);
            if (effectivePeriodFromPricing is null) continue;

            var effectivePeriodFromPricingInDays = effectivePeriodFromPricing.GetDifference().TotalDays;
            differenceInDays -= effectivePeriodFromPricingInDays;

            totalAmountWithCommissions += spotCustomPricing.PriceDaily.PricePerFootWithCommissions *
                                          (decimal)effectivePeriodFromPricingInDays;
        }

        if (differenceInDays > 0)
        {
            totalAmountWithCommissions +=
                (decimal)differenceInDays * spot.PriceDaily.PricePerFootWithCommissions;
        }

        if (includeElectricity && spot.Electricity is not null)
        {
            electricityPrice = (decimal)dateTimeRange.GetDifference().TotalDays * spot.Electricity.DailyPrice;
        }

        return new ReservationCost(totalAmountWithCommissions * chargeableBoatLength, electricityPrice);
    }

    private static ReservationCost CalculateWeeklyReservationCost(DateTimeRange dateTimeRange,
        List<SpotCustomPricing> spotCustomPricings,
        double differenceInWeeks, Spot spot, decimal chargeableBoatLength, bool includeElectricity)
    {
        var totalAmountWithCommissions = 0m;
        var electricityPrice = 0m;
        foreach (var spotCustomPricing in spotCustomPricings)
        {
            var effectivePeriodFromPricing = spotCustomPricing.EffectivePeriod.GetOverlapOrDefault(dateTimeRange);
            if (effectivePeriodFromPricing is null) continue;

            var effectivePeriodFromPricingInWeeks = effectivePeriodFromPricing.GetDifference().GetTotalWeeks();
            differenceInWeeks -= effectivePeriodFromPricingInWeeks;

            totalAmountWithCommissions += spotCustomPricing.PriceWeekly.PricePerFootWithCommissions *
                                          (decimal)effectivePeriodFromPricingInWeeks;
        }

        if (differenceInWeeks > 0)
        {
            totalAmountWithCommissions +=
                (decimal)differenceInWeeks * spot.PriceWeekly.PricePerFootWithCommissions;
        }

        if (includeElectricity && spot.Electricity is not null)
        {
            electricityPrice = (decimal)dateTimeRange.GetDifference().TotalDays * spot.Electricity.DailyPrice;
        }

        return new ReservationCost(totalAmountWithCommissions * chargeableBoatLength, electricityPrice);
    }

    private static ReservationCost CalculateMonthlyReservationCost(DateTimeRange dateTimeRange,
        List<SpotCustomPricing> spotCustomPricings,
        double differenceInMonths, Spot spot, decimal chargeableBoatLength, bool includeElectricity)
    {
        var totalAmountWithCommissions = 0m;
        var electricityPrice = 0m;
        foreach (var spotCustomPricing in spotCustomPricings)
        {
            var effectivePeriodFromPricing = spotCustomPricing.EffectivePeriod.GetOverlapOrDefault(dateTimeRange);
            if (effectivePeriodFromPricing is null) continue;

            var effectivePeriodFromPricingInMonths = effectivePeriodFromPricing.GetDifference().GetTotalMonths();
            differenceInMonths -= effectivePeriodFromPricingInMonths;

            totalAmountWithCommissions += spotCustomPricing.PriceMonthly.PricePerFootWithCommissions *
                                          (decimal)effectivePeriodFromPricingInMonths;
        }

        if (differenceInMonths > 0)
        {
            totalAmountWithCommissions +=
                (decimal)differenceInMonths * spot.PriceMonthly.PricePerFootWithCommissions;
        }

        if (includeElectricity && spot.Electricity is not null)
        {
            electricityPrice = (decimal)dateTimeRange.GetDifference().TotalDays * spot.Electricity.DailyPrice;
        }

        return new ReservationCost(totalAmountWithCommissions * chargeableBoatLength, electricityPrice);
    }
}