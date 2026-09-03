using System.Data;
using EcommerceAPI.Common.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class ErrorLogRepository : IErrorLogRepository
{
    private readonly string _connectionString;

    public ErrorLogRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task LogErrorAsync(
        string logLevel,
        string message,
        string exceptionType,
        string? stackTrace,
        string? innerException,
        string? requestPath,
        string? requestMethod,
        int statusCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("BhumikaEcom.usp_ErrorLog_Create", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@LogLevel", SqlDbType.NVarChar, 50).Value = logLevel;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, -1).Value = message;
            command.Parameters.Add("@ExceptionType", SqlDbType.NVarChar, 255).Value = exceptionType;
            command.Parameters.Add("@StackTrace", SqlDbType.NVarChar, -1).Value = (object?)stackTrace ?? DBNull.Value;
            command.Parameters.Add("@InnerException", SqlDbType.NVarChar, -1).Value = (object?)innerException ?? DBNull.Value;
            command.Parameters.Add("@RequestPath", SqlDbType.NVarChar, 500).Value = (object?)requestPath ?? DBNull.Value;
            command.Parameters.Add("@RequestMethod", SqlDbType.NVarChar, 10).Value = (object?)requestMethod ?? DBNull.Value;
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Value = statusCode;
            command.Parameters.Add("@MachineName", SqlDbType.NVarChar, 100).Value = Environment.MachineName;

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            // Fail silently so database logging failures don't crash the main error handler
        }
    }
}
