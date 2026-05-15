using BicycleShop.Application.DTOs.Orders;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequestDto>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}
