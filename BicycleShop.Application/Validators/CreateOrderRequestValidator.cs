using BicycleShop.Application.DTOs.Orders;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Order items are required.")
            .NotEmpty()
            .WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemRequestValidator());
    }
}
