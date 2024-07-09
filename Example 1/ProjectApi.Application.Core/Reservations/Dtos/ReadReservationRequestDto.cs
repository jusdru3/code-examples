using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using ProjectUtilities.Utilities.Data.PredicateBuilders.Dtos;
using ProjectApi.Core.Base.Dtos;
using ProjectApi.Data.Models.Reservations;

namespace ProjectApi.Application.Core.Reservations.Dtos;

public class ReadReservationRequestDto : BaseGetRequestDto
{
    public Guid? SpotId { get; set; }

    [property: JsonConverter(typeof(SmartEnumValueConverter<ReservationStatus, string>))]
    public ICollection<ReservationStatus>? Statuses { get; set; }

    public ReadReservationRequestDtoOrderBy OrderBy { get; set; }
    public OrderDirection OrderByDirection { get; set; } = OrderDirection.Descending;
}

public enum ReadReservationRequestDtoOrderBy
{
    NumericId,
}