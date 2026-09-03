using EcommerceAPI.DTOs.Cart;
using FluentValidation;
namespace EcommerceAPI.Validators;
public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}