namespace SnipeIt.Api.Dtos;

/// <summary>
/// User data returned to clients. Never includes password/security fields.
/// </summary>
public sealed class UserResponseDto
{
    public uint Id { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? EmployeeNum { get; set; }
    public string? Jobtitle { get; set; }
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
    public uint? CompanyId { get; set; }
    public int? ManagerId { get; set; }
    public bool Activated { get; set; }
    public bool? Vip { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Display names populated by the service.</summary>
    public string? DepartmentName { get; set; }
    public string? LocationName { get; set; }
    public string? CompanyName { get; set; }
}

/// <summary>Payload for creating a user.</summary>
public sealed class CreateUserDto
{
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }
    public string? EmployeeNum { get; set; }
    public string? Jobtitle { get; set; }
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
    public uint? CompanyId { get; set; }
    public int? ManagerId { get; set; }
    public bool Activated { get; set; } = true;
    public bool? Vip { get; set; }
}

/// <summary>Payload for updating a user. Null members are ignored (partial update).</summary>
public sealed class UpdateUserDto
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? EmployeeNum { get; set; }
    public string? Jobtitle { get; set; }
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
    public uint? CompanyId { get; set; }
    public int? ManagerId { get; set; }
    public bool? Vip { get; set; }
}

/// <summary>Self-service or admin change-password payload.</summary>
public sealed class ChangePasswordDto
{
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

/// <summary>Admin-triggered password reset payload.</summary>
public sealed class ResetPasswordDto
{
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

/// <summary>Enable/disable toggle payload.</summary>
public sealed class SetActivationDto
{
    public required bool Activated { get; set; }
}

/// <summary>Aggregated items assigned to a user.</summary>
public sealed class UserAssignedItemsDto
{
    public IReadOnlyList<AssignedAssetDto> Assets { get; set; } = Array.Empty<AssignedAssetDto>();
    public IReadOnlyList<AssignedLicenseDto> Licenses { get; set; } = Array.Empty<AssignedLicenseDto>();
    public IReadOnlyList<AssignedAccessoryDto> Accessories { get; set; } = Array.Empty<AssignedAccessoryDto>();
}

public sealed class AssignedAssetDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? AssetTag { get; set; }
    public string? Serial { get; set; }
}

public sealed class AssignedLicenseDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Serial { get; set; }
}

public sealed class AssignedAccessoryDto
{
    public uint Id { get; set; }
    public string? Name { get; set; }
}
