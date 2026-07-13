using FluentValidation;
using SnipeIt.Api.Dtos;

namespace SnipeIt.Api.Validators;

/// <summary>Validator for CreateUserDto.</summary>
public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(255).WithMessage("Username must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.LastName));
    }
}

/// <summary>Validator for UpdateUserDto (partial update — only validates provided fields).</summary>
public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.FirstName)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.LastName));
    }
}

/// <summary>Validator for ChangePasswordDto.</summary>
public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

/// <summary>Validator for ResetPasswordDto.</summary>
public sealed class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
