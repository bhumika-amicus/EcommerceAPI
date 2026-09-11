# EcommerceAPI

## 1. Project Overview
EcommerceAPI is a robust, highly-performant ASP.NET Core Web API designed to serve as the backend for an e-commerce platform. It provides endpoints for managing the full e-commerce lifecycle, including user authentication, product catalogs, shopping carts, checkout processing, mock payments, and order tracking. 

## 2. Technology Stack
The project is built using a modern, scalable .NET architecture, strictly utilizing the following technologies:
* **Framework:** ASP.NET Core 8 (C#)
* **Database Access:** ADO.NET with SQL Server
* **Database Logic:** Stored Procedures (for complex queries and aggregations)
* **Validation:** FluentValidation
* **Authentication:** JWT (JSON Web Tokens) with Refresh Tokens
* **Authorization:** Claims/Policy-based Authorization
* **Caching:** `IMemoryCache` and standard Response Caching
* **API Protection:** ASP.NET Core Rate Limiting middleware
* **Documentation:** Swagger/OpenAPI
* **Testing:** Postman & Newman

## 3. Architecture
The application follows a classic N-Tier Architecture.

### Request Flow
```text
Client/Postman
      ↓
Middleware (Rate Limiting, Exception Handling, Auth)
      ↓
Routing (API Versioning)
      ↓
Controller
      ↓
Service
      ↓
Repository
      ↓
ADO.NET / Stored Procedure
      ↓
SQL Server
      ↓
Repository
      ↓
Service
      ↓
Controller (Returns ApiResponse<T>)
      ↓
JSON Response
```

### Component Responsibilities
* **Controllers:** Handle HTTP routing, API versioning, attribute-based authorization, and return standardized `ApiResponse<T>` wrappers.
* **Services:** Contain the core business logic. They process DTOs, orchestrate validations, and call repositories.
* **Repositories:** Manage all database communication via ADO.NET and Stored Procedures using `SqlParameter` objects to prevent SQL injection.
* **DTOs:** Data Transfer Objects strictly define the shape of requests and responses.
* **Middlewares:** Handle cross-cutting concerns like Global Exception Handling and JWT validation.
* **Validators:** FluentValidation classes that validate incoming DTOs before business logic executes.
* **Common:** Houses shared utilities like Exceptions, the `ApiResponse` wrapper, and Security helpers.

Dependency Injection (DI) is heavily utilized. All Services and Repositories are registered with a `Scoped` lifetime in `Program.cs`.

## 4. Project Structure
```text
EcommerceAPI/
├── Controllers/         # API Endpoints (Auth, Products, Cart, Orders, etc.)
├── Services/            # Business logic and external clients (MockPaymentClient)
│   ├── Implementations/
│   └── Interfaces/
├── Repositories/        # ADO.NET Data access classes
│   ├── Implementations/
│   └── Interfaces/
├── DTOs/                # Request and Response data models
├── Models/              # Domain entities
├── Validators/          # FluentValidation rules for DTOs
├── Middlewares/         # GlobalExceptionMiddleware
├── Common/              # Shared classes (ApiResponse, Exceptions, Security)
├── Properties/          # launchSettings.json
├── Program.cs           # Application entry point & DI configuration
└── appsettings.json     # Configuration files
```

## 5. Authentication & Authorization
The API uses **JWT Authentication**.

* **Register & Login:** Users register and log in via the `AuthenticationController` to receive an `AccessToken` and a `RefreshToken`.
* **Access Tokens:** Short-lived tokens included in the `Authorization: Bearer <token>` header for protected endpoints.
* **Refresh Tokens:** Long-lived tokens used to silently obtain a new Access Token when the old one expires.
* **Authorization Policies:** Certain endpoints restrict access based on Claims. For example, `CanManageProducts` and `CanManageOrders` policies are implemented to restrict administrative endpoints to users possessing the appropriate administrative claims.

## 6. Caching
The application implements two caching strategies to improve performance:

1. **Response Caching:** Standard `[ResponseCache(Duration = X)]` is applied to heavy read-only endpoints (e.g., getting the catalog of Brands or Categories).
2. **Memory Cache:** `IMemoryCache` is utilized in services for frequently accessed, rarely changing data to prevent unnecessary database roundtrips. 

## 7. Rate Limiting
Global rate limiting is configured in `Program.cs` to prevent abuse:
* Authenticated users are partitioned by their `ClaimTypes.NameIdentifier` (User ID).
* Anonymous requests are partitioned by their IP Address.
* If a client exceeds the defined request limit within the configured time window, the server automatically returns an `HTTP 429 Too Many Requests` response.

## 8. Error Handling
The application uses a `GlobalExceptionMiddleware` to catch all unhandled exceptions and return a standardized JSON error response.

* **Business Exceptions:** Mapped to `400 Bad Request`.
* **NotFound Exceptions:** Mapped to `404 Not Found`.
* **Authentication/Authorization Exceptions:** Mapped to `401 Unauthorized` or `403 Forbidden`.
* **Unhandled Server Exceptions:** Mapped to `500 Internal Server Error`.

**Example Error Response:**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Email": ["Invalid email format."]
    }
}
```

## 9. Validation
Validation is achieved through **FluentValidation**.

* **API/Request Validation:** Incoming DTOs are validated against strict rules (e.g., maximum string lengths, required fields, numeric ranges).
* **Database Constraints:** `SqlParameter` objects are used in ADO.NET which inherently enforces database type constraints and prevents injection.

## 10. API Versioning
The API uses URL-based API versioning. All business endpoints (excluding Auth) are versioned.

Example: `/api/v1/products` and `/api/v2/products`

The system is configured with `AssumeDefaultVersionWhenUnspecified = true`, defaulting to `v1.0`.

## 11. API Endpoints Reference
A complete, detailed reference for every single API endpoint is available in the separate `docs/API_DOCUMENTATION.md` file.

Below is a high-level summary of the controllers:
* **Authentication:** `/api/auth` (Register, Login, Refresh)
* **Products:** `/api/v1/products` and `/api/v2/products` (Catalog management, availability, bulk operations)
* **Categories & Brands:** `/api/v1/categories` and `/api/v1/brands` (Read-only catalogs)
* **Cart:** `/api/v1/cart` and `/api/v2/cart` (Manage active user cart)
* **Checkout & Shipping:** `/api/v1/checkout` and `/api/v2/checkout` (Shipping methods and order placement)
* **Orders:** `/api/v1/orders` and `/api/v2/orders` (Order history, cancellation, and reordering)
* **Payments:** `/api/v1/orders/{orderId}/payments` and `/api/v1/mock-payments` (Payment processing simulation)
* **Address:** `/api/v1/address` (User shipping addresses)
* **ProductPrices:** `/api/v1/product-prices`

## 12. Database
The database interaction strictly avoids ORMs like Entity Framework in favor of high-performance **ADO.NET** and **Stored Procedures**.

* **Repository Pattern:** Isolates all SQL commands from the business logic.
* **Stored Procedures:** Used for complex operations, specifically the `usp_Product_GetPaged` procedure which handles highly-optimized searching, filtering, and paging for the product catalog.

## 13. Security
Security is a top priority in this implementation:
* **JWT Authentication:** Secure, stateless token validation.
* **Password Hashing:** Passwords are never stored in plain text.
* **Parameterized SQL:** Direct defense against SQL injection via `SqlParameter`.
* **Rate Limiting:** Defense against brute-force and DDoS attacks.
* **CORS:** Controlled Cross-Origin Resource Sharing rules.

## 14. Running the Project
1. **Configure Database:** Ensure SQL Server is running. Update the connection string in `appsettings.Development.json` (do NOT commit secrets to Git).
2. **Configure JWT:** Ensure a secure 32+ character JWT Key is present in your `appsettings.json`.
3. **Build:** Open the terminal in the root directory and run `dotnet build`.
4. **Run:** Execute `dotnet run`. The application will start.
5. **Swagger:** Navigate to `https://localhost:<port>/swagger` in your browser to view the interactive API documentation.

## 15. Postman Collection
A full Postman collection is provided for testing.
1. Import the `collection.json` file into Postman.
2. If environment variables are used, import the `environment.json` file and select it.
3. Run the **Auth -> Login** endpoint. Ensure you copy the returned `accessToken`.
4. Set the token as a Bearer Token on the Collection root, or directly on the protected requests.
5. You can now test Cart, Checkout, and Order endpoints!

## 16. Testing
The API can be tested manually via **Postman** or automatically via **Newman**.
* **Newman Execution:** You can run the entire collection headlessly from the terminal using `newman run collection.json -e environment.json -r htmlextra` to generate a beautiful HTML execution report of the entire API lifecycle.
