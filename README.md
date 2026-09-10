# E-Commerce API

## 📌 Overview
This is a robust, enterprise-grade E-Commerce RESTful API built with **ASP.NET Core (.NET 8)**. The project is meticulously designed with a strong focus on clean architecture, security, centralized error handling, and comprehensive audit logging. It successfully implements all requirements across three core assignment phases: ADO.NET Foundations, Security & Middleware, and Production-Ready Features.

## 🚀 Enterprise Features Implemented
* **Layered Architecture:** Clear separation of concerns utilizing Controllers, Services, Interfaces, and Repositories.
* **Security & Identity:** JWT-based authentication, secure BCrypt password hashing, and role-based access control (Admin vs. Standard User).
* **Declarative Audit & Error Logging:** Automatic tracking of critical business events (Order Creation, Payment processing, Cancellations) into the `AuditLogs` table. Global exception handling middleware traps 500-level errors and stores them in the `ErrorLogs` table while returning RFC 7807 Problem Details to the client.
* **API Versioning:** Supports URL-based API versioning (e.g., `/api/v1/...`, `/api/v2/...`) to strictly maintain backward compatibility across Product, Cart, Checkout, and Order APIs.
* **Performance & Caching:** 
  * **In-Memory Caching:** Used for frequently accessed taxonomy data (Categories).
  * **Response Caching (Client-Side):** Used for large payload endpoints (Image Downloads) to reduce server load.
* **Resilient Payment Integration:** Calls an external Mock Payment API to simulate transaction processing. Implements a critical failsafe to detect and log "Orphaned Payments" (when a card is charged successfully but the local database update fails).
* **Database Security:** Direct table access is strictly restricted. **100%** of all database operations communicate with SQL Server through **Stored Procedures** and custom Table Types using ADO.NET (`SqlDataReader` / `SqlCommand`).

---

## 🛤️ Comprehensive API Endpoints

### 1. Authentication & Users (`/api/auth`)
* `POST /api/auth/register` - Registers a new user with secure password hashing.
* `POST /api/auth/login` - Authenticates credentials and issues a JWT token.

### 2. Product Catalog & Versioning (`/api/v{version:apiVersion}/products`)
* `GET /` - Retrieves the paginated catalog. Supports advanced filtering and sorting:
  * *Query Params:* `search`, `categoryId`, `brandId`, `minPrice`, `maxPrice`, `minRating`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize`.
* `GET /{id}` - Retrieves details for a specific product.
* `POST /` - *(Admin)* Creates a new product.
* `PUT /{id}` - *(Admin)* Updates product details.
* `DELETE /{id}` - *(Admin)* Deletes a product.
* `GET /{id}/availability` - Checks real-time stock availability.
* `POST /availability/batch` - Checks stock for multiple items at once using a custom SQL Table Type.

### 3. Secure File Uploads (`/api/v{version:apiVersion}/products/{id}/image`)
* `POST /` - *(Admin)* Uploads a product image. Features deep security checks including file extension validation, size limits (5MB max), and **Magic Byte validation** to prevent spoofed files. 
* `GET /` - Downloads the product image. Utilizes `[ResponseCache]` to instruct the client browser to cache the image.

### 4. Taxonomy & Pricing
* `GET /api/categories` - Retrieves all categories. *(Cached In-Memory)*
* `GET /api/categories/{id}` - Retrieves a specific category.
* `GET /api/brands` - Retrieves all brands.
* `GET /api/brands/{id}` - Retrieves a specific brand.
* `GET /api/product-prices` - Retrieves current prices.
* `POST /api/product-prices` - *(Admin)* Sets or updates product pricing.

### 5. Shopping Cart (`/api/v{version:apiVersion}/cart`)
* `GET /` - Retrieves the active cart for the logged-in customer.
* `GET /count` - Gets the exact number of items currently in the cart.
* `GET /subtotal` - Calculates the raw subtotal of the cart items.
* `POST /items` - Adds a new item to the cart.
* `PUT /items/{id}` - Updates the quantity of a specific line item.
* `DELETE /items/{id}` - Removes a single item from the cart.
* `DELETE /` - Clears the entire cart.

### 6. Checkout Process (`/api/v{version:apiVersion}/checkout`)
* `GET /shipping-methods` - Lists all available shipping options and their respective fees.
* `POST /` - **Previews Checkout Calculation.** Validates the cart, checks real-time stock availability, and calculates the exact Tax, Shipping Charges, Subtotal, and Final Order Total before committing.

### 7. Orders & History (`/api/v{version:apiVersion}/orders`)
* `POST /` - **Creates Order.** Validates final stock, computes all totals, creates the order securely via SQL Transaction, and automatically **clears the customer's cart** upon success.
* `GET /` - Retrieves the customer's Order History. Supports pagination and filtering by Order Status.
* `GET /{id}` - Retrieves full details, line items, and totals for a specific order.
* `PUT /{id}/cancel` - Cancels an active order, logging the action securely in the Audit logs.
* `POST /{id}/reorder` - One-click reorder. Takes all items from a past order and drops them back into the active cart.

### 8. Mock Payment Integration (`/api/orders/{orderId}/payments`)
* `POST /` - Processes payment against the mock external API. If successful, stores the `TransactionReference` UUID. If the external mock API succeeds but the local DB fails, it fires a `LogCritical` warning into the `ErrorLogs` for immediate admin remediation.
* `GET /` - Retrieves payment history logs for a specific order.

---

## 🗄️ Database Stored Procedure Abstraction
All database interactions are completely isolated from C# via the `BhumikaEcom` schema. Key procedures include:
* **Core Logic:** `usp_Order_Create`, `usp_Payment_ProcessResult`, `usp_Product_ValidateBatchStock`
* **Logging:** `usp_AuditLog_Create`, `usp_ErrorLog_Create`
* **Cart Operations:** `usp_Cart_GetItemCount`, `usp_Cart_GetSubtotal`, `usp_Cart_Clear`

## 🛠️ Tech Stack & Dependencies
* **Framework:** ASP.NET Core Web API (.NET 8)
* **Architecture:** N-Tier Layered Architecture (Controllers -> Services -> Repositories)
* **Database:** Microsoft SQL Server
* **Authentication:** JSON Web Tokens (JWT) & BCrypt
* **Data Access:** ADO.NET (Raw SQL Connections)
* **Validation:** FluentValidation
* **Versioning:** `Asp.Versioning.Mvc`
