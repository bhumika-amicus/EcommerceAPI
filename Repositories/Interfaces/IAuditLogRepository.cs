namespace EcommerceAPI.Repositories;

public interface IAuditLogRepository
{
    Task LogAsync(
        string? userId,
        string action,
        string? entityName,
        string? entityId,
        string? ipAddress,
        string? userAgent,
        int statusCode,
        CancellationToken cancellationToken = default);
}
