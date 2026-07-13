using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;

namespace SnipeIt.Api.Services;

/// <summary>Service contract for user management operations.</summary>
public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetUsersAsync(PageQuery query, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> GetUserByIdAsync(uint id, CancellationToken cancellationToken = default);

    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> UpdateUserAsync(uint id, UpdateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a user (sets DeletedAt). Returns false if not found.</summary>
    Task<bool> DeleteUserAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>Sets a new BCrypt password. Returns false if the user was not found.</summary>
    Task<bool> ChangePasswordAsync(uint id, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>Enables or disables (activates/deactivates) a user. Returns false if not found.</summary>
    Task<UserResponseDto?> SetActivationAsync(uint id, bool activated, CancellationToken cancellationToken = default);

    /// <summary>Returns the assets, licenses and accessories assigned to a user.</summary>
    Task<UserAssignedItemsDto?> GetAssignedItemsAsync(uint id, CancellationToken cancellationToken = default);
}
