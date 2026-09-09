using System.Data;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Exceptions;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Models.Orders;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
namespace EcommerceAPI.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;
    public OrderRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<int> CreateOrderAsync(CreateOrderModel order, CancellationToken cancellationToken = default)
    {
        var table = new DataTable();
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("ProductName", typeof(string));
        table.Columns.Add("UnitPrice", typeof(decimal));
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("LineTotal", typeof(decimal));
        foreach (var item in order.Items)
        {
            table.Rows.Add(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal);
        }

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Order_Create", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@CustomerId", order.CustomerId);
        command.Parameters.AddWithValue("@ShippingMethodId", order.ShippingMethodId);
        command.Parameters.AddWithValue("@ShippingAddress", order.ShippingAddress);
        command.Parameters.AddWithValue("@Subtotal", order.Subtotal);
        command.Parameters.AddWithValue("@ShippingFee", order.ShippingFee);
        command.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
        command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);

        var itemsParameter = new SqlParameter("@Items", SqlDbType.Structured) { TypeName = "BhumikaEcom.OrderItemType", Value = table };

        command.Parameters.Add(itemsParameter);

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Order creation did not return an OrderId.");

        }
        return reader.GetInt32(reader.GetOrdinal("OrderId"));

    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Order_GetById", connection);
        command.CommandType = CommandType.StoredProcedure; command.Parameters.AddWithValue("@OrderId", orderId); await connection.OpenAsync(cancellationToken); await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken)) { return null; }

        // ----------------------------- // Result Set 1: Order Header// -----------------------------
        var order = new OrderDto
        {
            OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
            OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
            ShippingMethodId = reader.GetInt32(reader.GetOrdinal("ShippingMethodId")),
            ShippingMethod = reader.GetString(reader.GetOrdinal("ShippingMethod")),
            ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
            TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
            OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
            ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            Items = new List<OrderItemDto>()
        };

        // ----------------------------- // Result Set 2: Order Items // -----------------------------
        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            order.Items.Add(new OrderItemDto
            {
                OrderItemId = reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            });
        }
        return order;
    }

    public async Task<PagedResult<OrderDto>> GetCustomerOrdersPagedAsync(int customerId, OrderQueryDto query, CancellationToken cancellationToken = default)
    {
        var orders = new List<OrderDto>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Order_GetPaged", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = (object?)query.Search ?? DBNull.Value;
        command.Parameters.Add("@OrderStatus", SqlDbType.NVarChar, 30).Value = (object?)query.OrderStatus ?? DBNull.Value;

        var minPriceParam = command.Parameters.Add("@MinTotalAmount", SqlDbType.Decimal);
        minPriceParam.Precision = 18;
        minPriceParam.Scale = 2;
        minPriceParam.Value = (object?)query.MinTotalAmount ?? DBNull.Value;

        var maxPriceParam = command.Parameters.Add("@MaxTotalAmount", SqlDbType.Decimal);
        maxPriceParam.Precision = 18;
        maxPriceParam.Scale = 2;
        maxPriceParam.Value = (object?)query.MaxTotalAmount ?? DBNull.Value;

        command.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object?)query.FromDate ?? DBNull.Value;
        command.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = (object?)query.ToDate ?? DBNull.Value;

        command.Parameters.Add("@SortBy", SqlDbType.NVarChar, 20).Value = query.SortBy;
        command.Parameters.Add("@SortDirection", SqlDbType.NVarChar, 4).Value = query.SortDirection;
        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = query.PageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = query.PageSize;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int totalCount = 0;
        if (await reader.ReadAsync(cancellationToken))
        {
            totalCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new OrderDto
            {
                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                ShippingMethodId = reader.GetInt32(reader.GetOrdinal("ShippingMethodId")),
                ShippingMethod = reader.IsDBNull(reader.GetOrdinal("ShippingMethod")) ? string.Empty : reader.GetString(reader.GetOrdinal("ShippingMethod")),
                ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                ShippingAddress = reader.GetString(reader.GetOrdinal("ShippingAddress")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            });
        }

        return new PagedResult<OrderDto>
        {
            Items = orders,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand("BhumikaEcom.usp_Order_Cancel", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        catch (SqlException ex) when (ex.Number >= 50000)
        {
            if (ex.Number == 50012)
            {
                throw new NotFoundException(ex.Message);
            }
            throw new BusinessException(ex.Message);
        }
    }
}
