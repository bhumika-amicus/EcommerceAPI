
using System.Data;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Payments;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;

    public PaymentRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<PaymentDto> CreatePaymentAsync( int orderId, decimal amount, string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand(
            "BhumikaEcom.usp_Payment_Create",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Payment creation did not return payment information.");
        }

        return new PaymentDto
        {
            PaymentId = reader.GetInt32(
                reader.GetOrdinal("PaymentId")),

            OrderId = reader.GetInt32(
                reader.GetOrdinal("OrderId")),

            TransactionReference = reader.IsDBNull(
                reader.GetOrdinal("TransactionReference"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("TransactionReference")),

            Amount = reader.GetDecimal(
                reader.GetOrdinal("Amount")),

            PaymentStatus = reader.GetString(
                reader.GetOrdinal("PaymentStatus")),

            PaymentMethod = reader.GetString(
                reader.GetOrdinal("PaymentMethod")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("CreatedAt")),

            UpdatedAt = reader.IsDBNull(
                reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UpdatedAt"))
        };
    }


    public async Task ProcessPaymentResultAsync(int paymentId, int orderId, string paymentStatus, string? transactionReference, CancellationToken cancellationToken = default) {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Payment_ProcessResult", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@PaymentId", paymentId);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
        command.Parameters.AddWithValue("@TransactionReference", (object?)transactionReference ?? DBNull.Value);
        await connection.OpenAsync(cancellationToken); 
        
        await command.ExecuteNonQueryAsync(cancellationToken); }


    public async Task<PaymentDto?> GetPaymentByIdAsync(
        int paymentId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand(
            "BhumikaEcom.usp_Payment_GetById",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@PaymentId", paymentId);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new PaymentDto
        {
            PaymentId = reader.GetInt32(
                reader.GetOrdinal("PaymentId")),

            OrderId = reader.GetInt32(
                reader.GetOrdinal("OrderId")),

            TransactionReference = reader.IsDBNull(
                reader.GetOrdinal("TransactionReference"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("TransactionReference")),

            Amount = reader.GetDecimal(
                reader.GetOrdinal("Amount")),

            PaymentStatus = reader.GetString(
                reader.GetOrdinal("PaymentStatus")),

            PaymentMethod = reader.GetString(
                reader.GetOrdinal("PaymentMethod")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("CreatedAt")),

            UpdatedAt = reader.IsDBNull(
                reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UpdatedAt"))
        };
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var payments = new List<PaymentDto>();

        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand(
            "BhumikaEcom.usp_Payment_GetByOrderId",
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@OrderId", orderId);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            payments.Add(new PaymentDto
            {
                PaymentId = reader.GetInt32(
                    reader.GetOrdinal("PaymentId")),

                OrderId = reader.GetInt32(
                    reader.GetOrdinal("OrderId")),

                TransactionReference = reader.IsDBNull(
                    reader.GetOrdinal("TransactionReference"))
                        ? null
                        : reader.GetString(
                            reader.GetOrdinal("TransactionReference")),

                Amount = reader.GetDecimal(
                    reader.GetOrdinal("Amount")),

                PaymentStatus = reader.GetString(
                    reader.GetOrdinal("PaymentStatus")),

                PaymentMethod = reader.GetString(
                    reader.GetOrdinal("PaymentMethod")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt")),

                UpdatedAt = reader.IsDBNull(
                    reader.GetOrdinal("UpdatedAt"))
                        ? null
                        : reader.GetDateTime(
                            reader.GetOrdinal("UpdatedAt"))
            });
        }

        return payments;
    }




}
