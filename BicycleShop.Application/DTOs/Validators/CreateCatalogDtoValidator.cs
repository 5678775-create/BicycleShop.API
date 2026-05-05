using BicycleShop.Application.DTOs;
using FluentValidation;

namespace BicycleShop.Application.Validators;

public class CreateCatalogDtoValidator : AbstractValidator<CreateCatalogDto>
{
    public CreateCatalogDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва категорії обов'язкова.")
            .MaximumLength(100).WithMessage("Назва не може перевищувати 100 символів.");
    }
}