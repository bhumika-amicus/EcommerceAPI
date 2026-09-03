# E-Commerce API
## 📌 Overview
This is a robust, enterprise-grade E-Commerce RESTful API built with **ASP.NET Core**. The project is designed with a strong focus on clean architecture, security, centralized error handling, and comprehensive audit logging. It provides foundational e-commerce capabilities, including user authentication, product management, and cart operations.
## 🚀 Key Features
* **Layered Architecture:** Clear separation of concerns utilizing Controllers, Interfaces, and Repositories.
* **Security & Identity:** JWT-based authentication, secure password handling, and role-based access control (Admin vs. Standard User).
* **Declarative Audit Logging:** Uses custom Action Filters (the `[AuditLog]` attribute) to automatically track and record critical business and security events without cluttering business logic.
* **Global Exception Handling:** A custom `ExceptionHandlingMiddleware` ensures all API responses conform to RFC 7807 (Problem Details). To maintain performance and prevent database bloat, **only 500-level (Server) errors** are logged to the database.
* **Database Security:** Direct table access is restricted. All CRUD operations communicate with the SQL Server strictly through **Stored Procedures** within a custom database schema (`BhumikaEcom`).
---
## 🛤️ API Flows & Endpoints
### 1. Authentication (`/api/Authentication`)
Manages user onboarding and secure access.
* `POST /register` - Registers a new user. *(Audited: `USER_REGISTER`)*
* `POST /login` - Authenticates credentials and issues a JWT. *(Audited: `USER_LOGIN`)*
### 2. Product Management (`/api/Products`)
Manages the e-commerce inventory. Read operations are public, while modifications require Admin privileges.
* `GET /` - Retrieves the paginated catalog of products.
* `GET /{id}` - Retrieves details for a specific product.
* `POST /` - Adds a new product to the catalog. *(Audited: `PRODUCT_CREATE`)*
* `PUT /{id}` - Modifies an existing product's details. *(Audited: `PRODUCT_UPDATE`)*
* `DELETE /{id}` - Removes a product from the system. *(Audited: `PRODUCT_DELETE`)*
* `GET /{id}/availability` - Checks stock availability for a single product.
* `POST /availability/batch` - Checks stock availability for multiple products simultaneously.
### 3. Pricing (`/api/ProductPrices`)
Manages active price updates.
* `GET /` - Retrieves all product prices.
* `GET /{id}` - Retrieves pricing for a specific product.
* `POST /` - Updates or sets the price of an existing product. *(Audited: `PRICE_CHANGED`)*
### 4. Shopping Cart (`/api/Carts`)
Manages the customer's active shopping cart session.
* `GET /` - Retrieves the active cart for the authenticated customer.
* `GET /count` - Gets the total number of items in the cart.
* `GET /subtotal` - Gets the calculated subtotal of the cart.
* `POST /items` - Adds a new item to the cart.
* `PUT /items/{id}` - Updates the quantity of an existing cart item.
* `DELETE /items/{id}` - Removes an item from the cart.
* `DELETE /` - Clears all items from the cart.
### 5. Categories & Brands (`/api/Categories` & `/api/Brands`)
Read-only endpoints to retrieve taxonomy data.
* `GET /` - Retrieves all categories/brands.
* `GET /{id}` - Retrieves details for a specific category/brand.
---
## 🗄️ Database Stored Procedure Catalog
To ensure security and abstraction, the API executes all data-access operations using Stored Procedures under the `BhumikaEcom` schema. 
Here is the complete catalog of procedures developed for this system:
### 🛡️ System Logging
* **`BhumikaEcom.usp_AuditLog_Create`**: Safely records audited events triggered by the `[AuditLog]` attribute (e.g., successful logins, product modifications). Captures user identity, affected records, and client metadata (IP/User-Agent).
* **`BhumikaEcom.usp_ErrorLog_Create`**: Records critical application crashes (5xx Status Codes) intercepted by the global error middleware to help developers debug server-side failures rapidly.
### 👤 User Management
* **`BhumikaEcom.usp_User_Create`**: Securely inserts a new user record with a hashed password.
* **`BhumikaEcom.usp_User_GetByEmail`**: Retrieves a user by their email address during authentication/login.
### 📦 Product Management
* **`BhumikaEcom.usp_Product_GetPaged`**: Retrieves a paginated list of products for the storefront.
* **`BhumikaEcom.usp_Product_GetById`**: Retrieves a single product's detailed record.
* **`BhumikaEcom.usp_Product_Create`**: Inserts a new product into the inventory.
* **`BhumikaEcom.usp_Product_Update`**: Updates an existing product's metadata (Name, Description, etc).
* **`BhumikaEcom.usp_Product_Delete`**: Safely removes a product from the database.
* **`BhumikaEcom.usp_Product_GetAvailability`**: Checks the active stock levels for a product.
* **`BhumikaEcom.usp_Product_ValidateBatchStock`**: Evaluates stock levels for a batch of products using a Custom Table Type (`BhumikaEcom.BatchStockCheckType`).
### 💵 Product Pricing
* **`BhumikaEcom.usp_ProductPrice_GetAll`**: Fetches current prices across the catalog.
* **`BhumikaEcom.usp_ProductPrice_GetByProductId`**: Fetches the price specifically bound to a given Product ID.
### 🛒 Cart Operations
* **`BhumikaEcom.usp_Cart_GetByCustomerId`**: Retrieves the full cart and its items for a specific customer.
* **`BhumikaEcom.usp_Cart_GetItemCount`**: Quickly calculates the total quantity of items in a cart.
* **`BhumikaEcom.usp_Cart_GetSubtotal`**: Calculates the monetary subtotal of all items in a cart.
* **`BhumikaEcom.usp_Cart_AddItem`**: Inserts a new cart item or increments an existing one.
* **`BhumikaEcom.usp_Cart_UpdateQuantity`**: Adjusts the specific quantity of a cart item.
* **`BhumikaEcom.usp_Cart_RemoveItem`**: Deletes a specific line item from the cart.
* **`BhumikaEcom.usp_Cart_Clear`**: Wipes all items from a customer's active cart.
### 🏷️ Categories & Brands
* **`BhumikaEcom.usp_Category_GetAll`**: Retrieves all product categories.
* **`BhumikaEcom.usp_Category_GetById`**: Retrieves a single category.
* **`BhumikaEcom.usp_Brand_GetAll`**: Retrieves all product brands.
* **`BhumikaEcom.usp_Brand_GetById`**: Retrieves a single brand.
---
## 🛠️ Tech Stack
* **Framework:** .NET 8 (ASP.NET Core Web API)
* **Language:** C#
* **Database:** Microsoft SQL Server
* **Authentication:** JSON Web Tokens (JWT)
* **Data Access:** ADO.NET (using `SqlDataReader` / `SqlCommand`)
  
