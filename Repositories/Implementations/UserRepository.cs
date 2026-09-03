using System.Data;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Authentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<int> CreateUserAsync( RegisterDto dto, string passwordHash, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand( "BhumikaEcom.usp_User_Create", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100)
            .Value = dto.FullName;

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
            .Value = dto.Email;

        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500)
            .Value = passwordHash;

        await connection.OpenAsync(cancellationToken);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt32(result);
        }
        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            throw new ConflictException( "An account with this email already exists.");
        }
    }

    public async Task<UserLoginDataDto?> GetUserByEmailAsync( string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand( "BhumikaEcom.usp_User_GetByEmail",connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
            .Value = email;

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        UserLoginDataDto? user = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (user == null)
            {
                user = new UserLoginDataDto
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    Roles = new List<string>()
                };
            }

            user.Roles.Add( reader.GetString(reader.GetOrdinal("RoleName")));
        }

        return user;
    }
}