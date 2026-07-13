namespace SnipeIt.Api.Dtos;

public sealed class DashboardSummaryDto
{
    public int TotalAssets { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLicenses { get; set; }
    public int TotalAccessories { get; set; }
    
    public int AssetsDeployed { get; set; }
    public int AssetsReadyToDeploy { get; set; }
    public int AssetsPending { get; set; }
    public int AssetsArchived { get; set; }
    
    public IEnumerable<ActionLogResponseDto> RecentActivity { get; set; } = [];
}
