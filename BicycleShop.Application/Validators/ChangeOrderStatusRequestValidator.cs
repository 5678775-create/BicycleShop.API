using BicycleShop.Application.DTOs.Orders;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class ChangeOrderStatusRequestValidator : AbstractValidator<ChangeOrderStatusRequestDto>
{
    public ChangeOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("NewStatus must be a valid order status.");
    }
}
