using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>
/// Service interface for asset operations
/// </summary>
public interface IAssetService
{
    /// <summary>
    /// Gets all assets with pagination
    /// </summary>
    /// <param name="skipCount">Number of records to skip</param>
    /// <param name="takeCount">Number of records to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of assets</returns>
    Task<(IEnumerable<AssetResponseDto> Data, int Total)> GetAssetsAsync(
        int skipCount = 0,
        int takeCount = 50,
        int? assignedToUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asset by ID
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset DTO or null if not found</returns>
    Task<AssetResponseDto?> GetAssetByIdAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asset by asset tag
    /// </summary>
    /// <param name="assetTag">Asset tag</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset DTO or null if not found</returns>
    Task<AssetResponseDto?> GetAssetByTagAsync(string assetTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new asset
    /// </summary>
    /// <param name="createAssetDto">Create asset DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created asset DTO</returns>
    Task<AssetResponseDto> CreateAssetAsync(
        CreateAssetDto createAssetDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="updateAssetDto">Update asset DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset DTO or null if not found</returns>
    Task<AssetResponseDto?> UpdateAssetAsync(
        uint id,
        UpdateAssetDto updateAssetDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAssetAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out an asset to a user
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset DTO or null if not found</returns>
    Task<AssetResponseDto?> CheckoutAssetAsync(
        uint id,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks in an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<AssetResponseDto?> CheckinAssetAsync(uint id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the history of an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="skipCount">Records to skip</param>
    /// <param name="takeCount">Records to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated history</returns>
    Task<(IEnumerable<ActionLogResponseDto> Data, int Total)> GetAssetHistoryAsync(
        uint id,
        int skipCount = 0,
        int takeCount = 50,
        CancellationToken cancellationToken = default);
}
