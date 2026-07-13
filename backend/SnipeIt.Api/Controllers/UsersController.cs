using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;

namespace SnipeIt.Api.Controllers;

/// <summary>Controller for managing users.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ICurrentUserService currentUser,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets a paginated, searchable, sortable list of users (admin only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetUsers(
        [FromQuery] PageQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        var result = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single user. Admins can view anyone; users may view themselves.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetUser(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin && _currentUser.UserId != (int)id) return Forbid();

        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user is null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(user);
    }

    /// <summary>Creates a new user (admin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> CreateUser(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();

        var user = await _userService.CreateUserAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>Updates a user (admin only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(
        uint id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();

        var user = await _userService.UpdateUserAsync(id, dto, cancellationToken);
        if (user is null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(user);
    }

    /// <summary>Soft-deletes a user (admin only).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        if (_currentUser.UserId == (int)id)
        {
            return BadRequest(new { message = "You cannot delete your own account" });
        }

        var deleted = await _userService.DeleteUserAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { message = $"User with ID {id} not found" });
        return NoContent();
    }

    /// <summary>Changes a user's password. Admins may change anyone's; users may change their own.</summary>
    [HttpPost("{id}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        uint id,
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin && _currentUser.UserId != (int)id) return Forbid();

        var ok = await _userService.ChangePasswordAsync(id, dto.NewPassword, cancellationToken);
        if (!ok) return NotFound(new { message = $"User with ID {id} not found" });
        return NoContent();
    }

    /// <summary>Admin-triggered password reset.</summary>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        uint id,
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();

        var ok = await _userService.ChangePasswordAsync(id, dto.NewPassword, cancellationToken);
        if (!ok) return NotFound(new { message = $"User with ID {id} not found" });
        return NoContent();
    }

    /// <summary>Enables or disables a user (admin only).</summary>
    [HttpPatch("{id}/activation")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> SetActivation(
        uint id,
        [FromBody] SetActivationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin) return Forbid();
        if (_currentUser.UserId == (int)id && !dto.Activated)
        {
            return BadRequest(new { message = "You cannot disable your own account" });
        }

        var user = await _userService.SetActivationAsync(id, dto.Activated, cancellationToken);
        if (user is null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(user);
    }

    /// <summary>Gets assets, licenses and accessories assigned to a user.</summary>
    [HttpGet("{id}/items")]
    [ProducesResponseType(typeof(UserAssignedItemsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAssignedItemsDto>> GetUserItems(uint id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin && _currentUser.UserId != (int)id) return Forbid();

        var items = await _userService.GetAssignedItemsAsync(id, cancellationToken);
        if (items is null) return NotFound(new { message = $"User with ID {id} not found" });
        return Ok(items);
    }
}
