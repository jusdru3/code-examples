using ProjectApi.Application.Core.Reservations.Dtos;
using FluentValidation;

namespace ProjectApi.Application.Core.Reservations.Validators;

public class UpdateReservationRequestDtoValidator : AbstractValidator<CreateReservationRequestDto>
{
    public UpdateReservationRequestDtoValidator()
    {
    }
}