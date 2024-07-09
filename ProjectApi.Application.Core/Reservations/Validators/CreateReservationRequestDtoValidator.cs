using ProjectApi.Application.Core.Reservations.Dtos;
using ProjectApi.Core.Base.Validations;
using FluentValidation;

namespace ProjectApi.Application.Core.Reservations.Validators;

public class CreateReservationRequestDtoValidator : AbstractValidator<CreateReservationRequestDto>
{
    public CreateReservationRequestDtoValidator()
    {
        RuleFor(e => e.DateTimeRange).LocalOrUnspecified();
    }
}