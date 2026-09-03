using System.Data;
using EcommerceAPI.Common.Options;
using EcommerceAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly string _connectionString;

    public BrandRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<IEnumerable<Brand>> GetAllBrandsAsync(CancellationToken cancellationToken = default)
    {
        var brands = new List<Brand>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Brand_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            brands.Add(new Brand
            {
                BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            });
        }

        return brands;
    }

    public async Task<Brand?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Brand_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@BrandId", SqlDbType.Int).Value = brandId;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Brand
        {
            BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
        };
    }
}
