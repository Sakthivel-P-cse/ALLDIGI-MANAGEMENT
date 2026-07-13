using System.Text.Json;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Common;

/// <summary>
/// Writes audit trail entries to the existing <see cref="ActionLog"/> table (reused from
/// the Laravel schema rather than introducing a new table). Captures who/what/when plus
/// optional old/new value snapshots serialized into the <c>LogMeta</c> JSON column.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Records an action. Actor id, IP and user-agent are pulled from the current request.
    /// </summary>
    /// <param name="actionType">Verb, e.g. "create", "update", "delete", "checkout".</param>
    /// <param name="itemType">Entity name, e.g. "Asset", "User".</param>
    /// <param name="itemId">Affected entity id.</param>
    /// <param name="note">Optional human-readable note.</param>
    /// <param name="oldValues">Optional pre-change snapshot (serialized to LogMeta).</param>
    /// <param name="newValues">Optional post-change snapshot (serialized to LogMeta).</param>
    /// <param name="targetId">Optional secondary target id (e.g. user an asset was checked out to).</param>
    /// <param name="targetType">Optional secondary target type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogAsync(
        string actionType,
        string itemType,
        int itemId,
        string? note = null,
        object? oldValues = null,
        object? newValues = null,
        int? targetId = null,
        string? targetType = null,
        CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class AuditLogService : IAuditLogService
{
    private readonly SnipeitContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        SnipeitContext context,
        ICurrentUserService currentUser,
        ILogger<AuditLogService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogAsync(
        string actionType,
        string itemType,
        int itemId,
        string? note = null,
        object? oldValues = null,
        object? newValues = null,
        int? targetId = null,
        string? targetType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? logMeta = null;
            if (oldValues is not null || newValues is not null)
            {
                logMeta = JsonSerializer.Serialize(new { old = oldValues, @new = newValues });
            }

            var now = DateTime.UtcNow;
            var entry = new ActionLog
            {
                ActionType = actionType,
                ItemType = itemType,
                ItemId = itemId,
                TargetId = targetId,
                TargetType = targetType,
                CreatedBy = _currentUser.UserId,
                Note = note,
                LogMeta = logMeta,
                RemoteIp = _currentUser.RemoteIp,
                UserAgent = _currentUser.UserAgent,
                ActionSource = "web",
                ActionDate = now,
                CreatedAt = now,
                UpdatedAt = now,
                Quantity = 1
            };

            _context.ActionLogs.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Auditing must never break the primary operation — log and swallow.
            _logger.LogError(ex,
                "Failed to write audit log for {ActionType} on {ItemType} {ItemId}",
                actionType, itemType, itemId);
        }
    }
}
