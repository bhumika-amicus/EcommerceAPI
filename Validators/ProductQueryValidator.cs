
using EcommerceAPI.DTOs.Products;
using FluentValidation;

namespace EcommerceAPI.Validators;

public class ProductQueryValidator : AbstractValidator<ProductQueryDto>
{  
    public ProductQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("Minimum price cannot be negative.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue)
            .WithMessage("Maximum price cannot be negative.");

        RuleFor(x => x)
            .Must(x =>
                !x.MinPrice.HasValue ||
                !x.MaxPrice.HasValue ||
                x.MinPrice <= x.MaxPrice)
            .WithMessage("Minimum price cannot be greater than maximum price.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("Minimum rating must be between 0 and 5.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortBy)
            .WithMessage("SortBy must be one of: name, price, rating.");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .WithMessage("SortDirection must be either asc or desc.");
    }

    private static bool BeValidSortBy(string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => true,
            "price" => true,
            "rating" => true,
            _ => false
        };
    }

    private static bool BeValidSortDirection(string? sortDirection)
    {
        return sortDirection?.ToLowerInvariant() switch
        {
            "asc" => true,
            "desc" => true,
            _ => false
        };
    }
}