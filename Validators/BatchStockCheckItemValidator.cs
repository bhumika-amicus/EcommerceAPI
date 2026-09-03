using EcommerceAPI.DTOs.Products;
using FluentValidation;

namespace EcommerceAPI.Validators;

public class BatchStockCheckItemValidator : AbstractValidator<BatchStockCheckItemDto>
{
    public BatchStockCheckItemValidator()
    {

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.RequestedQuantity)
            .GreaterThan(0).WithMessage("Requested quantity must be at least 1.");
    }
}
