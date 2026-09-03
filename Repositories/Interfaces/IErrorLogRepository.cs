namespace EcommerceAPI.Repositories;
public interface IErrorLogRepository
{
    Task LogErrorAsync(
        string logLevel,
        string message,
        string exceptionType,
        string? stackTrace,
        string? innerException,
        string? requestPath,
        string? requestMethod,
        int statusCode,
        CancellationToken cancellationToken = default);
}