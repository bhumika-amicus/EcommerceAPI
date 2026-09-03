using EcommerceAPI.Common.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AuditLogAttribute : TypeFilterAttribute
{
    public AuditLogAttribute(string action, string entityName = "")  : base(typeof(AuditLogFilter))
    {
        Arguments = new object[] { action, entityName };
    }
}
