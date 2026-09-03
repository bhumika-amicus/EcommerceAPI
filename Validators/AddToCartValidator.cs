using EcommerceAPI.DTOs.Cart;
using FluentValidation;
namespace EcommerceAPI.Validators;
public class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid Product ID is required.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Please select a quantity of at least 1.");
    }
}