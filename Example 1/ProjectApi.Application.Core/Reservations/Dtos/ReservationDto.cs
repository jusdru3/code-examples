using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using ProjectApi.Application.Core.Boats.Dtos;
using ProjectApi.Application.Core.Spots.Dtos;
using ProjectApi.Data.Models.Reservations;
using ProjectApi.Data.Models.Reservations.OwnedTypes;
using ProjectApi.Data.ValueObjects;

namespace ProjectApi.Application.Core.Reservations.Dtos;

public record ReservationDto(
    long NumericId,
    Guid Id,
    Guid SpotId,
    Guid ApplicationUserId,
    Guid BoatId,
    DateTimeRange DateTimeRange,
    DateTimeRange DateTimeRangeInUtc,
    [property: JsonConverter(typeof(SmartEnumValueConverter<ReservationStatus, string>))]
    ReservationStatus Status,
    ReservationCost Cost,
    DateTime? DraftExpirationTime,
    SpotDto Spot,
    BoatDto Boat,
    ReservationTerminationDto? Termination,
    ICollection<ReservationBillingPeriodDto> BillingPeriods,
    bool IncludesCustomPeriod
);

public record ReservationTerminationDto(
    DateTime TerminatedOn,
    [property: JsonConverter(typeof(SmartEnumValueConverter<ReservationTerminationReason, string>))]
    ReservationTerminationReason Reason
);