using System.Data;
using System.Text.Json;
using EcommerceAPI.Common;
using EcommerceAPI.Common.Options;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IOptions<DatabaseOptions> dbOptions)
    {
        _connectionString = dbOptions.Value.DefaultConnection;
    }

    public async Task<PagedResult<ProductDetailModel>> GetAllProductsAsync(ProductQueryDto query, CancellationToken cancellationToken = default)
    {
        var products = new List<ProductDetailModel>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_GetPaged", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = (object?)query.Search ?? DBNull.Value;
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = (object?)query.CategoryId ?? DBNull.Value;
        command.Parameters.Add("@BrandId", SqlDbType.Int).Value = (object?)query.BrandId ?? DBNull.Value;

        var minPriceParameter = command.Parameters.Add("@MinPrice", SqlDbType.Decimal);
        minPriceParameter.Precision = 18;
        minPriceParameter.Scale = 2;
        minPriceParameter.Value = (object?)query.MinPrice ?? DBNull.Value;

        var maxPriceParameter = command.Parameters.Add("@MaxPrice", SqlDbType.Decimal);
        maxPriceParameter.Precision = 18;
        maxPriceParameter.Scale = 2;
        maxPriceParameter.Value = (object?)query.MaxPrice ?? DBNull.Value;

        var minRatingParameter = command.Parameters.Add("@MinRating", SqlDbType.Decimal);
        minRatingParameter.Precision = 2;
        minRatingParameter.Scale = 1;
        minRatingParameter.Value = (object?)query.MinRating ?? DBNull.Value;

        command.Parameters.Add("@SortBy", SqlDbType.NVarChar, 20).Value = query.SortBy;
        command.Parameters.Add("@SortDirection", SqlDbType.NVarChar, 4).Value = query.SortDirection;
        command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = query.PageNumber;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = query.PageSize;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var totalCount = 0;
        if (await reader.ReadAsync(cancellationToken))
        {
            totalCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new ProductDetailModel
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
                BrandName = reader.GetString(reader.GetOrdinal("BrandName")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.IsDBNull(reader.GetOrdinal("StockQuantity"))? 0 : reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath"))

            });
        }

        return new PagedResult<ProductDetailModel>
        {
            Items = products,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDetailModel?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new ProductDetailModel
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
                BrandName = reader.GetString(reader.GetOrdinal("BrandName")),
                Rating = reader.GetDecimal(reader.GetOrdinal("Rating")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                StockQuantity = reader.IsDBNull(reader.GetOrdinal("StockQuantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath"))
            };
        }

        return null;
    }

    public async Task<int> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_Create", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Name", dto.Name);
        command.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
        command.Parameters.AddWithValue("@BrandId", dto.BrandId);

        var priceParam = command.Parameters.Add("@Price", SqlDbType.Decimal);
        priceParam.Precision = 18;
        priceParam.Scale = 2;
        priceParam.Value = dto.Price;
        var ratingParam = command.Parameters.Add("@Rating", SqlDbType.Decimal);
        ratingParam.Precision = 2;
        ratingParam.Scale = 1;
        ratingParam.Value = dto.Rating;
        command.Parameters.AddWithValue("@StockQuantity", dto.StockQuantity);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
    public async Task<bool> UpdateProductAsync(int productId, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_Update", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Name", dto.Name);
        command.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", dto.CategoryId);
        command.Parameters.AddWithValue("@BrandId", dto.BrandId);

        var priceParam = command.Parameters.Add("@Price", SqlDbType.Decimal);
        priceParam.Precision = 18;
        priceParam.Scale = 2;
        priceParam.Value = dto.Price;

        var ratingParam = command.Parameters.Add("@Rating", SqlDbType.Decimal);
        ratingParam.Precision = 2;
        ratingParam.Scale = 1;
        ratingParam.Value = dto.Rating;

        command.Parameters.AddWithValue("@StockQuantity", dto.StockQuantity);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_Delete", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@ProductId", productId);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
        return rowsAffected > 0;
    }

    public async Task<ProductAvailabilityDto?> GetProductAvailabilityAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("BhumikaEcom.usp_Product_GetAvailability", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@ProductId", productId);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new ProductAvailabilityDto
            {
                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                IsInStock = reader.GetBoolean(reader.GetOrdinal("IsInStock"))
            };
        }
        return null;
    }
    public async Task<IEnumerable<BatchStockValidationResultDto>> ValidateBatchStockAsync(IEnumerable<BatchStockCheckItemDto> items, CancellationToken cancellationToken = default)
    {
        var results = new List<BatchStockValidationResultDto>(items.Count());
        var table = new DataTable();

        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("RequestedQuantity", typeof(int));

        foreach (var item in items)
        {
            table.Rows.Add(item.ProductId, item.RequestedQuantity);
        }
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand( "BhumikaEcom.usp_Product_ValidateBatchStock", connection);

        command.CommandType = CommandType.StoredProcedure;

        var parameter = new SqlParameter("@Items", SqlDbType.Structured)
        {
            TypeName = "BhumikaEcom.BatchStockCheckType",
            Value = table
        };

        command.Parameters.Add(parameter);

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int productIdOrdinal = reader.GetOrdinal("ProductId");
        int nameOrdinal = reader.GetOrdinal("ProductName");
        int requestedQtyOrdinal = reader.GetOrdinal("RequestedQuantity");
        int availableStockOrdinal = reader.GetOrdinal("AvailableStock");
        int isAvailableOrdinal = reader.GetOrdinal("IsAvailable");
        int messageOrdinal = reader.GetOrdinal("Message");

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BatchStockValidationResultDto
            {
                ProductId = reader.GetInt32(productIdOrdinal),
                ProductName = reader.IsDBNull(nameOrdinal) ? "Unknown Product" : reader.GetString(nameOrdinal),
                RequestedQuantity = reader.GetInt32(requestedQtyOrdinal),
                AvailableStock = reader.GetInt32(availableStockOrdinal),
                IsAvailable = reader.GetBoolean(isAvailableOrdinal),
                Message = reader.IsDBNull(messageOrdinal) ? string.Empty : reader.GetString(messageOrdinal)
            });
        }

        return results;
    }


   
    public async Task<bool> UpdateImagePathAsync( int productId,  string imagePath,
        CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);

            await using var command = new SqlCommand(
                "BhumikaEcom.usp_Product_UpdateImagePath",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

            command.Parameters.Add(
                "@ImagePath",
                SqlDbType.NVarChar,
                500).Value = imagePath;

            await connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            int rowsAffected = result != null
                ? Convert.ToInt32(result)
                : 0;

            return rowsAffected > 0;
    }


    public async Task<BulkCreateProductResponseDto> BulkCreateProductsAsync( List<CreateProductDto> products, CancellationToken cancellationToken = default)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("RowNumber", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));
            dataTable.Columns.Add("CategoryId", typeof(int));
            dataTable.Columns.Add("BrandId", typeof(int));
            dataTable.Columns.Add("Price", typeof(decimal));
            dataTable.Columns.Add("Rating", typeof(decimal));
            dataTable.Columns.Add("StockQuantity", typeof(int));

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];

                dataTable.Rows.Add(
                    i + 1,
                    product.Name,
                    product.Description ?? string.Empty,
                    product.CategoryId,
                    product.BrandId,
                    product.Price,
                    product.Rating,
                    product.StockQuantity
                );
            }


            await using var connection = new SqlConnection(_connectionString);

            await using var command = new SqlCommand(
                "BhumikaEcom.usp_Product_BulkCreate",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            var parameter = command.Parameters.AddWithValue(
                "@Products",
                dataTable);

            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = "BhumikaEcom.ProductBulkType";


            await connection.OpenAsync(cancellationToken);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var validationResults = new List<BulkProductValidationResultDto>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var result = new BulkProductValidationResultDto
                {
                    RowNumber = reader.GetInt32(reader.GetOrdinal("RowNumber")),
                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                    IsValid = reader.GetBoolean(reader.GetOrdinal("IsValid")),
                    Errors = new List<string>()
                };

                var errorsRaw = reader.IsDBNull(reader.GetOrdinal("Errors")) ? null : reader.GetString(reader.GetOrdinal("Errors"));

                if (!string.IsNullOrWhiteSpace(errorsRaw))
                {
                    result.Errors = errorsRaw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                validationResults.Add(result);
            }

            return new BulkCreateProductResponseDto
            {
                TotalRecords = validationResults.Count,
                SuccessfulRecords = validationResults.Count(x => x.IsValid),
                FailedRecords = validationResults.Count(x => !x.IsValid),
                Results = validationResults
            };
    }


    public async Task<BulkInventoryUpdateResponseDto> BulkUpdateInventoryAsync( List<BulkInventoryUpdateItemDto> items,
        CancellationToken cancellationToken = default)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("RowNumber", typeof(int));
            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("StockQuantity", typeof(int));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                dataTable.Rows.Add(
                    i + 1,
                    item.ProductId,
                    item.StockQuantity
                );
            }

            var results = new List<BulkInventoryUpdateResultDto>();

            using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand(
                "BhumikaEcom.usp_Product_BulkUpdateInventory",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            var parameter = command.Parameters.Add(
                "@Items",
                SqlDbType.Structured);

            parameter.TypeName = "BhumikaEcom.BulkInventoryUpdateType";
            parameter.Value = dataTable;

            await connection.OpenAsync(cancellationToken);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new BulkInventoryUpdateResultDto
                {
                    RowNumber = reader.GetInt32(
                        reader.GetOrdinal("RowNumber")),

                    ProductId = reader.GetInt32(
                        reader.GetOrdinal("ProductId")),

                    StockQuantity = reader.GetInt32(
                        reader.GetOrdinal("StockQuantity")),

                    IsValid = reader.GetBoolean(
                        reader.GetOrdinal("IsValid")),

                    ErrorMessage = reader.IsDBNull(
                        reader.GetOrdinal("ErrorMessage"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal("ErrorMessage"))
                });
            }

            return new BulkInventoryUpdateResponseDto
            {
                TotalRecords = results.Count,
                SuccessfulRecords = results.Count(x => x.IsValid),
                FailedRecords = results.Count(x => !x.IsValid),
                Results = results
            };
        }



}
