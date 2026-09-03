using System.Data;
using EcommerceAPI.Common.Options;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class ProductPriceRepository : IProductPriceRepository
{
    private readonly string _connectionString;

    public ProductPriceRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<IEnumerable<ProductPrice>> GetAllProductPricesAsync(CancellationToken cancellationToken = default)
    {
        var productPrices = new List<ProductPrice>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_ProductPrice_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            productPrices.Add(new ProductPrice
            {
                ProductPriceId = reader.GetInt32(reader.GetOrdinal("ProductPriceId")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            });
        }

        return productPrices;
    }

    public async Task<ProductPrice?> GetProductPriceByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_ProductPrice_GetByProductId", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ProductPrice
        {
            ProductPriceId = reader.GetInt32(reader.GetOrdinal("ProductPriceId")),
            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price"))
        };
    }
}
