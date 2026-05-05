using BicycleShop.Application.DTOs;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Назва обов'язкова.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Ціна має бути більше 0.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}