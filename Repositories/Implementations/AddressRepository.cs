using System.Data;
using EcommerceAPI.DTOs.Addresses;
using EcommerceAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using EcommerceAPI.Common.Options;

namespace EcommerceAPI.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly string _connectionString;

    public AddressRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<Address?> GetAddressByUserIdAsync(  int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand(  "BhumikaEcom.usp_Address_GetByUserId",  connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new Address
            {
                AddressId = reader.GetInt32(reader.GetOrdinal("AddressId")),

                UserId = reader.GetInt32( reader.GetOrdinal("UserId")),

                AddressLine1 = reader.GetString( reader.GetOrdinal("AddressLine1")),

                AddressLine2 = reader.IsDBNull( reader.GetOrdinal("AddressLine2"))? null : reader.GetString( reader.GetOrdinal("AddressLine2")),

                City = reader.GetString( reader.GetOrdinal("City")),

                State = reader.GetString( reader.GetOrdinal("State")),

                PostalCode = reader.GetString( reader.GetOrdinal("PostalCode")),

                Country = reader.GetString( reader.GetOrdinal("Country")),

                CreatedAt = reader.GetDateTime( reader.GetOrdinal("CreatedAt")),

                UpdatedAt = reader.IsDBNull( reader.GetOrdinal("UpdatedAt"))  ? null : reader.GetDateTime( reader.GetOrdinal("UpdatedAt"))
            };
        }

        return null;
    }


    public async Task<int> CreateAddressAsync( int userId ,  AddressReqDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand(
            "BhumikaEcom.usp_Address_Create",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        command.Parameters.Add("@AddressLine1", SqlDbType.NVarChar, 250).Value = dto.AddressLine1;

        command.Parameters.Add("@AddressLine2", SqlDbType.NVarChar, 250).Value = (object?)dto.AddressLine2 ?? DBNull.Value;

        command.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = dto.City;

        command.Parameters.Add("@State", SqlDbType.NVarChar, 100).Value = dto.State;

        command.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 20).Value = dto.PostalCode;

        command.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = dto.Country;

        await connection.OpenAsync(cancellationToken);

        var result =  await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }


    public async Task<bool> UpdateAddressAsync( int userId, AddressReqDto dto,  CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand( "BhumikaEcom.usp_Address_Update", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        command.Parameters.Add("@AddressLine1", SqlDbType.NVarChar, 250).Value = dto.AddressLine1;

        command.Parameters.Add("@AddressLine2", SqlDbType.NVarChar, 250).Value = (object?)dto.AddressLine2 ?? DBNull.Value;

        command.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = dto.City;

        command.Parameters.Add("@State", SqlDbType.NVarChar, 100).Value = dto.State;

        command.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 20).Value = dto.PostalCode;

        command.Parameters.Add("@Country", SqlDbType.NVarChar, 100).Value = dto.Country;

        await connection.OpenAsync(cancellationToken);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        var rowsAffected = result != null ? Convert.ToInt32(result): 0;

        return rowsAffected > 0;
    }


    public async Task<bool> DeleteAddressAsync( int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand( "BhumikaEcom.usp_Address_Delete", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync(cancellationToken);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        var rowsAffected = result != null ? Convert.ToInt32(result): 0;

        return rowsAffected > 0;
    }
}

