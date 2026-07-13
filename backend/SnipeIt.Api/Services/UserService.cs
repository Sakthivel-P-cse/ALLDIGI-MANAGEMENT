using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Common;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Services;

/// <summary>User management service. Follows the AssetService reference pattern.</summary>
public sealed class UserService : IUserService
{
    private readonly SnipeitContext _context;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _audit;
    private readonly ILogger<UserService> _logger;

    public UserService(
        SnipeitContext context,
        IMapper mapper,
        IAuditLogService audit,
        ILogger<UserService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<UserResponseDto>> GetUsersAsync(
        PageQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _context.Users.AsNoTracking().Where(u => u.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(u =>
                (u.Username != null && u.Username.Contains(term)) ||
                (u.FirstName != null && u.FirstName.Contains(term)) ||
                (u.LastName != null && u.LastName.Contains(term)) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.EmployeeNum != null && u.EmployeeNum.Contains(term)));
        }

        q = q.ApplySort(query.SortBy, query.IsDescending, u => u.Id);

        var total = await q.CountAsync(cancellationToken);
        var users = await q.Skip(query.Skip).Take(query.Take).ToListAsync(cancellationToken);

        var dtos = users.Select(u => _mapper.Map<UserResponseDto>(u)).ToList();
        await PopulateDisplayNamesAsync(dtos, cancellationToken);

        return new PagedResult<UserResponseDto>(dtos, query.Skip, query.Take, total);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(uint id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);

        if (user is null) return null;

        var dto = _mapper.Map<UserResponseDto>(user);
        await PopulateDisplayNamesAsync(new[] { dto }, cancellationToken);
        return dto;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Username == dto.Username && u.DeletedAt == null, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Username '{dto.Username}' already exists");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Username = dto.Username,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DisplayName = BuildDisplayName(dto.FirstName, dto.LastName, dto.Username),
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            EmployeeNum = dto.EmployeeNum,
            Jobtitle = dto.Jobtitle,
            Phone = dto.Phone,
            DepartmentId = dto.DepartmentId,
            LocationId = dto.LocationId,
            CompanyId = dto.CompanyId,
            ManagerId = dto.ManagerId,
            Activated = dto.Activated,
            Vip = dto.Vip,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("create", "User", (int)user.Id, note: $"Created user {user.Username}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("User {Username} created with ID {UserId}", user.Username, user.Id);
        return await GetUserByIdAsync(user.Id, cancellationToken) ?? _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(uint id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
        {
            var taken = await _context.Users
                .AnyAsync(u => u.Username == dto.Username && u.Id != id && u.DeletedAt == null, cancellationToken);
            if (taken)
            {
                throw new InvalidOperationException($"Username '{dto.Username}' already exists");
            }
            user.Username = dto.Username;
        }

        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.EmployeeNum != null) user.EmployeeNum = dto.EmployeeNum;
        if (dto.Jobtitle != null) user.Jobtitle = dto.Jobtitle;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.DepartmentId.HasValue) user.DepartmentId = dto.DepartmentId;
        if (dto.LocationId.HasValue) user.LocationId = dto.LocationId;
        if (dto.CompanyId.HasValue) user.CompanyId = dto.CompanyId;
        if (dto.ManagerId.HasValue) user.ManagerId = dto.ManagerId;
        if (dto.Vip.HasValue) user.Vip = dto.Vip;

        user.DisplayName = BuildDisplayName(user.FirstName, user.LastName, user.Username);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("update", "User", (int)user.Id, note: $"Updated user {user.Username}",
            cancellationToken: cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteUserAsync(uint id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null) return false;

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("delete", "User", (int)user.Id, note: $"Deleted user {user.Username}",
            cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(uint id, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null) return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("update", "User", (int)user.Id, note: "Password changed",
            cancellationToken: cancellationToken);
        return true;
    }

    public async Task<UserResponseDto?> SetActivationAsync(uint id, bool activated, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (user is null) return null;

        user.Activated = activated;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(activated ? "enable" : "disable", "User", (int)user.Id,
            note: activated ? "User enabled" : "User disabled", cancellationToken: cancellationToken);

        return await GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<UserAssignedItemsDto?> GetAssignedItemsAsync(uint id, CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
        if (!userExists) return null;

        var intId = (int)id;

        var assets = await _context.Assets.AsNoTracking()
            .Where(a => a.AssignedTo == intId && a.DeletedAt == null)
            .Select(a => new AssignedAssetDto { Id = a.Id, Name = a.Name, AssetTag = a.AssetTag, Serial = a.Serial })
            .ToListAsync(cancellationToken);

        var licenseIds = await _context.LicenseSeats
            .Where(ls => ls.AssignedTo == intId && ls.LicenseId.HasValue)
            .Select(ls => (uint)ls.LicenseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var licenses = await _context.Licenses.AsNoTracking()
            .Where(l => licenseIds.Contains(l.Id))
            .Select(l => new AssignedLicenseDto { Id = l.Id, Name = l.Name, Serial = l.Serial })
            .ToListAsync(cancellationToken);

        var accessoryIds = await _context.AccessoriesCheckouts
            .Where(ac => ac.AssignedTo == intId && ac.AccessoryId.HasValue)
            .Select(ac => (uint)ac.AccessoryId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var accessories = await _context.Accessories.AsNoTracking()
            .Where(a => accessoryIds.Contains(a.Id))
            .Select(a => new AssignedAccessoryDto { Id = a.Id, Name = a.Name })
            .ToListAsync(cancellationToken);

        return new UserAssignedItemsDto
        {
            Assets = assets,
            Licenses = licenses,
            Accessories = accessories
        };
    }

    private async Task PopulateDisplayNamesAsync(IReadOnlyList<UserResponseDto> dtos, CancellationToken cancellationToken)
    {
        if (dtos.Count == 0) return;

        var deptIds = dtos.Where(d => d.DepartmentId.HasValue).Select(d => (uint)d.DepartmentId!.Value).Distinct().ToList();
        var locIds = dtos.Where(d => d.LocationId.HasValue).Select(d => (uint)d.LocationId!.Value).Distinct().ToList();
        var compIds = dtos.Where(d => d.CompanyId.HasValue).Select(d => d.CompanyId!.Value).Distinct().ToList();

        var depts = deptIds.Count > 0
            ? await _context.Departments.Where(d => deptIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken)
            : new Dictionary<uint, string>();

        var locs = locIds.Count > 0
            ? await _context.Locations.Where(l => locIds.Contains(l.Id)).ToDictionaryAsync(l => l.Id, l => l.Name ?? string.Empty, cancellationToken)
            : new Dictionary<uint, string>();

        var comps = compIds.Count > 0
            ? await _context.Companies.Where(c => compIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            : new Dictionary<uint, string>();

        foreach (var dto in dtos)
        {
            if (dto.DepartmentId.HasValue && depts.TryGetValue((uint)dto.DepartmentId.Value, out var dName)) dto.DepartmentName = dName;
            if (dto.LocationId.HasValue && locs.TryGetValue((uint)dto.LocationId.Value, out var lName)) dto.LocationName = lName;
            if (dto.CompanyId.HasValue && comps.TryGetValue(dto.CompanyId.Value, out var cName)) dto.CompanyName = cName;
        }
    }

    private static string BuildDisplayName(string? first, string? last, string? username)
    {
        var full = string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrWhiteSpace(full) ? (username ?? string.Empty) : full;
    }
}
