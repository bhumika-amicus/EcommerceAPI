using System.Data;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Checkout;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories
{
    public class ShippingMethodRepository : IShippingMethodRepository
    {
        private readonly string _connectionString;

        public ShippingMethodRepository(IOptions<DatabaseOptions> dbOptions)
        {
            _connectionString = dbOptions.Value.DefaultConnection;
        }

        public async Task<ShippingMethodDto?> GetShippingMethodByIdAsync( int shippingMethodId, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);

            await using var command = new SqlCommand( "BhumikaEcom.usp_ShippingMethod_GetById", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue( "@ShippingMethodId", shippingMethodId);

            await connection.OpenAsync(cancellationToken);

            await using var reader =  await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new ShippingMethodDto
            {
                ShippingMethodId = reader.GetInt32( reader.GetOrdinal("ShippingMethodId")),

                Name = reader.GetString( reader.GetOrdinal("Name")),

                Description = reader.IsDBNull( reader.GetOrdinal("Description")) ? null : reader.GetString( reader.GetOrdinal("Description")),

                Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),

                EstimatedDeliveryDays = reader.GetInt32( reader.GetOrdinal("EstimatedDeliveryDays"))
            };
        }

        public async Task<IEnumerable<ShippingMethodDto>> GetAllShippingMethodsAsync(CancellationToken cancellationToken = default)
        {
            var shippingMethods = new List<ShippingMethodDto>();

            await using var connection = new SqlConnection(_connectionString);

            await using var command = new SqlCommand("BhumikaEcom.usp_ShippingMethod_GetAll", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                shippingMethods.Add(new ShippingMethodDto
                {
                    ShippingMethodId = reader.GetInt32(reader.GetOrdinal("ShippingMethodId")),

                    Name = reader.GetString(reader.GetOrdinal("Name")),

                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),

                    Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),

                    EstimatedDeliveryDays = reader.GetInt32(reader.GetOrdinal("EstimatedDeliveryDays"))
                });
            }

            return shippingMethods;
        }

    }

}
