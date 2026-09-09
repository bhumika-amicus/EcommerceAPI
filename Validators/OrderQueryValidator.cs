using EcommerceAPI.DTOs.Orders;
using FluentValidation;

namespace EcommerceAPI.Validators;

public class OrderQueryValidator : AbstractValidator<OrderQueryDto>
{
    public OrderQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.MinTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinTotalAmount.HasValue)
            .WithMessage("Minimum total amount cannot be negative.");

        RuleFor(x => x.MaxTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxTotalAmount.HasValue)
            .WithMessage("Maximum total amount cannot be negative.");

        RuleFor(x => x)
            .Must(x =>
                !x.MinTotalAmount.HasValue ||
                !x.MaxTotalAmount.HasValue ||
                x.MinTotalAmount <= x.MaxTotalAmount)
            .WithMessage("Minimum total amount cannot be greater than maximum total amount.");

        RuleFor(x => x)
            .Must(x =>
                !x.FromDate.HasValue ||
                !x.ToDate.HasValue ||
                x.FromDate <= x.ToDate)
            .WithMessage("From date cannot be later than to date.");

        RuleFor(x => x.OrderStatus)
            .Must(BeValidOrderStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.OrderStatus))
            .WithMessage("OrderStatus must be one of: Pending, Confirmed, Cancelled.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortBy)
            .WithMessage("SortBy must be one of: orderid, date, total, status, ordernumber.");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .WithMessage("SortDirection must be either asc or desc.");
    }

    private static bool BeValidOrderStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" => true,
            "confirmed" => true,
            "cancelled" => true,
            _ => false
        };
    }

    private static bool BeValidSortBy(string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "orderid" => true,
            "date" => true,
            "total" => true,
            "status" => true,
            "ordernumber" => true,
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
