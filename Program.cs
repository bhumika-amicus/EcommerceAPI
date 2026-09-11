
using System.Threading.RateLimiting;
using System;
using System.Text;
using EcommerceAPI.Common.Options;
using EcommerceAPI.Mappings;
using EcommerceAPI.Middlewares;
using EcommerceAPI.Repositories;
using EcommerceAPI.Services;
using EcommerceAPI.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Asp.Versioning;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Key), "Jwt:Key is required.")
    .ValidateOnStart();

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultConnection), "ConnectionStrings:DefaultConnection is required.")
    .ValidateOnStart();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Add services to the container.
builder.Services.AddControllers();

//Add API Versioning
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });


//add caching 
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

//add rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
        {
            var userId = httpContext.User.FindFirst("sub")?.Value
                         ?? httpContext.User.FindFirst("userId")?.Value;

            var partitionKey = userId != null 
                ? $"user:{userId}" 
                : $"anonymous:{httpContext.Connection.RemoteIpAddress}";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }
        else
        {
            context.HttpContext.Response.Headers.RetryAfter = "60";
        }

        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});


builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddValidatorsFromAssemblyContaining<ProductQueryValidator>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT options are not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),

            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services
    .AddAuthorization(options =>{
    options.AddPolicy("CanManageProducts", policy =>
    {
        policy.RequireClaim("CanManageProducts", "true");
    });

    options.AddPolicy("CanManageOrders", policy =>
    {
        policy.RequireClaim("CanManageOrders", "true");
    });
});


// Register Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped< IShippingMethodRepository, ShippingMethodRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

// Register Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductPriceService, ProductPriceService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IShippingMethodService,ShippingMethodService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();


builder.Services.AddHttpClient<IMockPaymentClient, MockPaymentClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7266/");
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EcommerceAPI", Version = "v1" });

    // Configure Swagger to ask for a JWT token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure Named CORS Policy for Frontend Application (Task 6)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Standard React App URL
                "http://localhost:5173",  // Standard Vite / React URL
                "http://localhost:4200")  // Standard Angular URL
              .AllowAnyHeader()           // Allows Content-Type, Authorization
              .AllowAnyMethod()           // Allows GET, POST, PUT, DELETE, OPTIONS
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Cache preflight response
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception handler (Task 4)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request logging middleware (Task 3)
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

// CORS Policy (Task 6)
app.UseCors("AllowFrontendApp");


app.UseAuthentication();

app.UseRateLimiter();

// Response Caching Middleware 
app.UseResponseCaching();

app.UseAuthorization();

app.MapControllers();

app.Run();
