using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Services;
using System.Text;
using System.Globalization;

namespace SnipeIt.Api.Controllers;

/// <summary>
/// Controller for managing assets
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ILogger<AssetsController> _logger;

    /// <summary>
    /// Initializes a new instance of the AssetsController
    /// </summary>
    /// <param name="assetService">Asset service</param>
    /// <param name="logger">Logger instance</param>
    public AssetsController(IAssetService assetService, ILogger<AssetsController> logger)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all assets with pagination
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0)</param>
    /// <param name="take">Number of records to take (default: 50, max: 500)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of assets</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AssetResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResponse<AssetResponseDto>>> GetAssets(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (skip < 0 || take < 1 || take > 500)
            {
                return BadRequest(new { message = "Invalid pagination parameters" });
            }

            int? assignedToUserId = null;
            if (!User.IsInRole("admin"))
            {
                var idClaim = User.FindFirst("id")?.Value;
                if (int.TryParse(idClaim, out int userId))
                {
                    assignedToUserId = userId;
                }
            }

            var (assets, total) = await _assetService.GetAssetsAsync(skip, take, assignedToUserId, cancellationToken);

            var response = new PaginatedResponse<AssetResponseDto>
            {
                Data = assets,
                Skip = skip,
                Take = take,
                Total = total
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            return StatusCode(500, new { message = "An error occurred while retrieving assets" });
        }
    }

    /// <summary>
    /// Gets an asset by ID
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset DTO</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> GetAsset(
        uint id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _assetService.GetAssetByIdAsync(id, cancellationToken);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with ID {id} not found" });
            }

            if (!User.IsInRole("admin"))
            {
                var idClaim = User.FindFirst("id")?.Value;
                if (int.TryParse(idClaim, out int userId) && asset.AssignedTo != userId)
                {
                    return Forbid();
                }
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the asset" });
        }
    }

    /// <summary>
    /// Gets an asset by asset tag
    /// </summary>
    /// <param name="assetTag">Asset tag</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset DTO</returns>
    [HttpGet("tag/{assetTag}")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> GetAssetByTag(
        string assetTag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _assetService.GetAssetByTagAsync(assetTag, cancellationToken);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with tag {assetTag} not found" });
            }

            if (!User.IsInRole("admin"))
            {
                var idClaim = User.FindFirst("id")?.Value;
                if (int.TryParse(idClaim, out int userId) && asset.AssignedTo != userId)
                {
                    return Forbid();
                }
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset by tag {AssetTag}", assetTag);
            return StatusCode(500, new { message = "An error occurred while retrieving the asset" });
        }
    }

    /// <summary>
    /// Creates a new asset
    /// </summary>
    /// <param name="createAssetDto">Create asset DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created asset DTO</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> CreateAsset(
        [FromBody] CreateAssetDto createAssetDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var asset = await _assetService.CreateAssetAsync(createAssetDto, cancellationToken);
            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating asset");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            return StatusCode(500, new { message = "An error occurred while creating the asset" });
        }
    }

    /// <summary>
    /// Updates an existing asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="updateAssetDto">Update asset DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset DTO</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> UpdateAsset(
        uint id,
        [FromBody] UpdateAssetDto updateAssetDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var asset = await _assetService.UpdateAssetAsync(id, updateAssetDto, cancellationToken);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with ID {id} not found" });
            }

            return Ok(asset);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating asset {AssetId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the asset" });
        }
    }

    /// <summary>
    /// Deletes an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAsset(uint id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var result = await _assetService.DeleteAssetAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = $"Asset with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the asset" });
        }
    }

    /// <summary>
    /// Checks out an asset to a user
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="request">Checkout request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset DTO</returns>
    [HttpPost("{id}/checkout")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> CheckoutAsset(
        uint id,
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var asset = await _assetService.CheckoutAssetAsync(id, request.UserId, cancellationToken);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with ID {id} not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while checking out the asset" });
        }
    }

    /// <summary>
    /// Checks in an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset DTO</returns>
    [HttpPost("{id}/checkin")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AssetResponseDto>> CheckinAsset(
        uint id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var asset = await _assetService.CheckinAssetAsync(id, cancellationToken);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with ID {id} not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while checking in the asset" });
        }
    }

    /// <summary>
    /// Gets the history of an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="skip">Number of records to skip (default: 0)</param>
    /// <param name="take">Number of records to take (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated history</returns>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(PaginatedResponse<ActionLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResponse<ActionLogResponseDto>>> GetAssetHistory(
        uint id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            var (history, total) = await _assetService.GetAssetHistoryAsync(id, skip, take, cancellationToken);
            var response = new PaginatedResponse<ActionLogResponseDto>
            {
                Data = history,
                Skip = skip,
                Take = take,
                Total = total
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving asset history" });
        }
    }

    /// <summary>
    /// Exports all assets to a CSV file
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV File</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportCsv(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!User.IsInRole("admin")) return Forbid();

            // Just get all for export, or we could pass large skip/take
            var (assets, _) = await _assetService.GetAssetsAsync(0, 100000, null, cancellationToken);

            var builder = new StringBuilder();
            builder.AppendLine("Id,AssetTag,Name,ModelName,Serial,StatusId,AssignedToName,LocationName,SupplierName,PurchaseDate,PurchaseCost,CreatedAt");

            foreach (var a in assets)
            {
                builder.AppendLine($"{a.Id},{EscapeCsv(a.AssetTag)},{EscapeCsv(a.Name)},{EscapeCsv(a.ModelName)},{EscapeCsv(a.Serial)},{a.StatusId},{EscapeCsv(a.AssignedToName)},{EscapeCsv(a.LocationName)},{EscapeCsv(a.SupplierName)},{a.PurchaseDate:yyyy-MM-dd},{a.PurchaseCost},{a.CreatedAt:yyyy-MM-dd}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"assets_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting assets to CSV");
            return StatusCode(500, new { message = "An error occurred while exporting assets" });
        }
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

/// <summary>
/// Checkout request model
/// </summary>
public sealed class CheckoutRequest
{
    /// <summary>Gets or sets the user ID</summary>
    public required int UserId { get; set; }
}

/// <summary>
/// Generic paginated response
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public sealed class PaginatedResponse<T>
{
    /// <summary>Gets or sets the data</summary>
    public IEnumerable<T>? Data { get; set; }

    /// <summary>Gets or sets the number of records skipped</summary>
    public int Skip { get; set; }

    /// <summary>Gets or sets the number of records taken</summary>
    public int Take { get; set; }

    /// <summary>Gets or sets the total number of records</summary>
    public int Total { get; set; }
}
