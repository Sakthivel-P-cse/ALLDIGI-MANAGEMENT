using FluentValidation;
using SnipeIt.Api.Dtos;

namespace SnipeIt.Api.Validators;

/// <summary>
/// Validator for UpdateAssetDto
/// </summary>
public sealed class UpdateAssetDtoValidator : AbstractValidator<UpdateAssetDto>
{
    /// <summary>
    /// Initializes a new instance of the UpdateAssetDtoValidator
    /// </summary>
    public UpdateAssetDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255).WithMessage("Asset name must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.AssetTag)
            .MaximumLength(255).WithMessage("Asset tag must not exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$").WithMessage("Asset tag must contain only alphanumeric characters, hyphens, and underscores")
            .When(x => !string.IsNullOrEmpty(x.AssetTag));

        RuleFor(x => x.ModelId)
            .GreaterThan(0).WithMessage("Model ID must be greater than 0")
            .When(x => x.ModelId.HasValue);

        RuleFor(x => x.Serial)
            .MaximumLength(255).WithMessage("Serial number must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Serial));

        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Purchase date cannot be in the future")
            .When(x => x.PurchaseDate.HasValue);

        RuleFor(x => x.AssetEolDate)
            .GreaterThanOrEqualTo(x => x.PurchaseDate)
            .WithMessage("End-of-life date must be after purchase date")
            .When(x => x.AssetEolDate.HasValue && x.PurchaseDate.HasValue);

        RuleFor(x => x.PurchaseCost)
            .GreaterThan(0).WithMessage("Purchase cost must be greater than 0")
            .When(x => x.PurchaseCost.HasValue);

        RuleFor(x => x.WarrantyMonths)
            .GreaterThan(0).WithMessage("Warranty months must be greater than 0")
            .When(x => x.WarrantyMonths.HasValue);

        RuleFor(x => x.AssignedTo)
            .GreaterThan(0).WithMessage("Assigned user ID must be greater than 0")
            .When(x => x.AssignedTo.HasValue);

        RuleFor(x => x.StatusId)
            .GreaterThan(0).WithMessage("Status ID must be greater than 0")
            .When(x => x.StatusId.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier ID must be greater than 0")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.RtdLocationId)
            .GreaterThan(0).WithMessage("RTD Location ID must be greater than 0")
            .When(x => x.RtdLocationId.HasValue);

        RuleFor(x => x.LocationId)
            .GreaterThan(0).WithMessage("Location ID must be greater than 0")
            .When(x => x.LocationId.HasValue);
    }
}
