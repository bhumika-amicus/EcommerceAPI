using EcommerceAPI.DTOs.Products;
using FluentValidation;

namespace EcommerceAPI.Validators;

public class BatchStockRequestValidator : AbstractValidator<BatchStockRequestDto>
{
    public BatchStockRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Request body cannot be null.")
            .NotEmpty()
            .WithMessage("Item list cannot be empty.")
            .Must(items => items.Count <= 50)
            .WithMessage("Maximum 50 items allowed per batch availability check.")
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate Product IDs are not allowed in a single batch request.");

        RuleForEach(x => x.Items)
            .SetValidator(new BatchStockCheckItemValidator());
    }
}
