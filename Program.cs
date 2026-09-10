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


builder.Services.AddHttpClient<IMockPaymentClient, MockPaymentClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7266/");
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Response Caching Middleware 
app.UseResponseCaching();

// CORS Policy (Task 6)
app.UseCors("AllowFrontendApp");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
