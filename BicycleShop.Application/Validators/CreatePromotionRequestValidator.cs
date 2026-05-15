using BicycleShop.Application.DTOs.Promotions;
using BicycleShop.Domain.Enums;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class CreatePromotionRequestValidator : AbstractValidator<CreatePromotionRequestDto>
{
    public CreatePromotionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Promotion name is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Promotion type must be valid.");

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0)
            .WithMessage("Discount percent must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount percent cannot be greater than 100.");

        RuleFor(x => x.ActiveTo)
            .GreaterThan(x => x.ActiveFrom)
            .WithMessage("ActiveTo must be later than ActiveFrom.");

        RuleFor(x => x.CatalogId)
            .NotEmpty()
            .When(x => x.Type == PromotionType.CategoryBased)
            .WithMessage("CatalogId is required for category based promotions.");
    }
}
