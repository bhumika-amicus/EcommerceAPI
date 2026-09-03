using System.Data;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Cart;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class CartRepository : ICartRepository
{
    private readonly string _connectionString;

    public CartRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<CartDto> GetCartByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var cartDto = new CartDto { CustomerId = customerId };

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_GetByCustomerId", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int cartItemIdOrdinal = reader.GetOrdinal("CartItemId");
        int productIdOrdinal = reader.GetOrdinal("ProductId");
        int productNameOrdinal = reader.GetOrdinal("ProductName");
        int priceAtAdditionOrdinal = reader.GetOrdinal("UnitPriceAtAddition");
        int currentPriceOrdinal = reader.GetOrdinal("CurrentPrice");
        int quantityOrdinal = reader.GetOrdinal("Quantity");
        int itemSubtotalOrdinal = reader.GetOrdinal("ItemSubtotal");

        while (await reader.ReadAsync(cancellationToken))
        {
            cartDto.Items.Add(new CartItemDto
            {
                CartItemId = reader.GetInt32(cartItemIdOrdinal),
                ProductId = reader.GetInt32(productIdOrdinal),
                ProductName = reader.GetString(productNameOrdinal),
                UnitPriceAtAddition = reader.GetDecimal(priceAtAdditionOrdinal),
                CurrentPrice = reader.GetDecimal(currentPriceOrdinal),
                Quantity = reader.GetInt32(quantityOrdinal),
                ItemSubtotal = reader.GetDecimal(itemSubtotalOrdinal)
            });
        }

        return cartDto;
    }

    public async Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_GetItemCount", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    public async Task<decimal> GetCartSubtotalAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_GetSubtotal", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CustomerId", customerId);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
    }

    public async Task AddItemToCartAsync(int customerId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_AddItem", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@ProductId", dto.ProductId);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateCartItemQuantityAsync(int customerId, int productId, UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_UpdateQuantity", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveCartItemAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_RemoveItem", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@ProductId", productId);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
        return rowsAffected > 0;
    }

    public async Task<bool> ClearCartAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Cart_Clear", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@CustomerId", customerId);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
        return rowsAffected > 0;
    }
}
