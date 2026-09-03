using System.Security.Claims;
using EcommerceAPI.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcommerceAPI.Common.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly string _action;
    private readonly string _entityName;

    public AuditLogFilter(IAuditLogRepository auditLogRepository, string action, string entityName)
    {
        _auditLogRepository = auditLogRepository;
        _action = action;
        _entityName = entityName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the controller action first
        var executedContext = await next();

        // Only record audit log if the HTTP request succeeded (< 400)
        if (executedContext.Exception == null && executedContext.HttpContext.Response.StatusCode < 400)
        {
            var httpContext = executedContext.HttpContext;

            // Extract User identity (Name / Email / Claim)
            var userId = httpContext.User?.Identity?.Name 
                      ?? httpContext.User?.FindFirst("sub")?.Value 
                      ?? httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.User?.FindFirst(ClaimTypes.Email)?.Value;

            // Fallback for unauthenticated endpoints (e.g. Login & Register)
            if (string.IsNullOrEmpty(userId))
            {
                foreach (var arg in context.ActionArguments.Values)
                {
                    if (arg is EcommerceAPI.DTOs.Authentication.LoginDto loginDto)
                    {
                        userId = loginDto.Email;
                        break;
                    }
                    if (arg is EcommerceAPI.DTOs.Authentication.RegisterDto registerDto)
                    {
                        userId = registerDto.Email;
                        break;
                    }
                }
            }

            // Extract Client IP address and User-Agent
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            // Extract EntityId from Route parameters (e.g. /api/products/{id} or /api/products/{productId})
            string? entityId = context.RouteData.Values["id"]?.ToString() 
                            ?? context.RouteData.Values["productId"]?.ToString();

            // Write to AuditLogs Database Table
            await _auditLogRepository.LogAsync(
                userId: userId,
                action: _action,
                entityName: _entityName,
                entityId: entityId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                statusCode: httpContext.Response.StatusCode
            );
        }
    }
}
