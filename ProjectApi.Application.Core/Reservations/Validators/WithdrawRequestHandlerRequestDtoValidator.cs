using ProjectApi.Application.Core.Reservations.Dtos;
using FluentValidation;

namespace ProjectApi.Application.Core.Reservations.Validators;

public class WithdrawRequestHandlerRequestDtoValidator : AbstractValidator<WithdrawRequestHandlerRequestDto>
{
    public WithdrawRequestHandlerRequestDtoValidator()
    {
    }
}