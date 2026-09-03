using System.Data;
using EcommerceAPI.Common.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task LogAsync(
        string? userId,
        string action,
        string? entityName,
        string? entityId,
        string? ipAddress,
        string? userAgent,
        int statusCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("BhumikaEcom.usp_AuditLog_Create", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = (object?)userId ?? DBNull.Value;
            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
            command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 100).Value = (object?)entityName ?? DBNull.Value;
            command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value = (object?)entityId ?? DBNull.Value;
            command.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 50).Value = (object?)ipAddress ?? DBNull.Value;
            command.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = (object?)userAgent ?? DBNull.Value;
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Value = statusCode;

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            // Fail silently so database logging failures do not crash the primary HTTP request
        }
    }
}
