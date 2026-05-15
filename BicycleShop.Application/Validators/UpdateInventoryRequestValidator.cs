using BicycleShop.Application.DTOs.Inventory;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class UpdateInventoryRequestValidator : AbstractValidator<UpdateInventoryRequestDto>
{
    public UpdateInventoryRequestValidator()
    {
        RuleFor(x => x.QuantityAvailable)
            .GreaterThanOrEqualTo(0)
            .WithMessage("QuantityAvailable cannot be negative.");
    }
}
