# ECommerce API

A comprehensive ASP.NET Core REST API for an e-commerce platform with user management, product catalog, shopping cart, checkout, and order management features.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Error Handling](#error-handling)
- [Security](#security)
- [Logging](#logging)
- [Performance Optimization](#performance-optimization)
- [Future Enhancements](#future-enhancements)

## Features

- **User Management**
  - User registration and authentication
  - User profile updates
  - Password reset functionality
  - JWT-based authentication

- **Product Management**
  - Complete product catalog
  - Category management
  - Advanced search and filtering (by name, brand, category)
  - Product counting and inventory tracking

- **Shopping Cart**
  - Add/remove items from cart
  - Update item quantities
  - Automatic total calculation

- **Checkout & Payment**
  - Stripe payment integration
  - Secure payment session creation
  - Order confirmation

- **Order Management**
  - Place orders from cart
  - Order history tracking
  - Order status management
  - Order item details

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Payment Gateway:** Stripe
- **API Documentation:** Swagger/OpenAPI
- **Logging:** Built-in .NET Core logging

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code
- Stripe account (for payment integration)
- Git (for version control)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/deepakchandarm/ecommerce.git
cd online-shop-api
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Install Required NuGet Packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Stripe.net
dotnet add package Swashbuckle.AspNetCore
```

## Configuration

### Update appsettings.json

1. **Database Connection String**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=OnlineShopDb;Trusted_Connection=true;Encrypt=false;"
   }
   ```

2. **Stripe Settings**
   ```json
   "StripeSettings": {
     "SecretKey": "sk_test_your_stripe_secret_key",
     "PublishableKey": "pk_test_your_stripe_publishable_key",
     "SuccessUrl": "https://yourdomain.com/checkout/success",
     "CancelUrl": "https://yourdomain.com/checkout/cancel"
   }
   ```

### Port Configuration

Edit `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5000",
      "launchBrowser": true,
      "launchUrl": "swagger/index.html"
    }
  }
}
```

## Database Setup

### 1. Create Initial Migration

```bash
dotnet ef migrations add InitialCreate
```

### 2. Apply Migration to Database

```bash
dotnet ef database update
```

### 3. Verify Database Creation

Check SQL Server to confirm `OnlineShopDb` database was created with all tables:
- Users
- Categories
- Products
- Carts
- CartItems
- Orders
- OrderItems

### Adding More Migrations

```bash
# After making model changes
dotnet ef migrations add DescriptionOfChanges
dotnet ef database update
```

### Rollback Database

```bash
dotnet ef database update PreviousMigrationName
```

## Running the Application

### Using Visual Studio

1. Set `WebApi` as the startup project
2. Press `F5` to debug or `Ctrl+F5` to run without debugging
3. Browser will automatically open to Swagger UI at `https://localhost:5117/swagger/index.html`

### Using Command Line

```bash
# Run in development mode
dotnet run

# Run in specific environment
dotnet run --environment Production
```

## API Documentation

### Swagger UI

Once the application is running, access the interactive API documentation at:

```
http://localhost:5117/swagger/index.html
https://localhost:7132/swagger/index.html
```

## Project Structure

```
WebApi/
├── Controllers/
│   ├── UserController.cs
│   ├── CategoryController.cs
│   ├── ProductController.cs
│   ├── CartItemController.cs
│   ├── CheckoutController.cs
│   └── OrderController.cs
├── Interface/
│   ├── IUserService.cs
│   ├── ICategoryService.cs
│   ├── IProductService.cs
│   ├── ICartService.cs
│   ├── ICartItemService.cs
│   ├── ICheckoutService.cs
│   └── IOrderService.cs
├── Services/
│   ├── UserService.cs
│   ├── CategoryService.cs
│   ├── ProductService.cs
│   ├── CartService.cs
│   ├── CartItemService.cs
│   ├── CheckoutService.cs
│   └── OrderService.cs
├── Dao/
│   ├── User.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── Cart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── Dtos/
│   ├── UserDto.cs
│   ├── ProductDto.cs
│   ├── OrderDto.cs
│   └── ... (other DTOs)
├── Data/
│   └── ApplicationDbContext.cs
├── Exceptions/
│   ├── ResourceNotFoundException.cs
│   ├── AlreadyExistException.cs
│   └── ... (other exceptions)
├── Configuration/
│   └── StripeSettings.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── Program.cs
```

## Database Schema

### Entity Relationships

```
User (1) ──── (Many) Cart
User (1) ──── (Many) Order
Category (1) ──── (Many) Product
Product (1) ──── (Many) CartItem
Product (1) ──── (Many) OrderItem
Cart (1) ──── (Many) CartItem
Order (1) ──── (Many) OrderItem
```

### Key Entities

#### User
- Stores user account information
- Related to Carts and Orders
- Email is unique

#### Product
- Contains product details (name, brand, price, quantity)
- Links to Category
- Tracked in inventory

#### Cart
- Shopping cart per user
- Contains CartItems
- Tracks total amount

#### Order
- Customer orders
- Contains OrderItems
- Tracks status and dates

#### Category
- Product categorization
- One category can have many products

## API Endpoints

### Users
- `POST /api/v1/users/add` - Create new user
- `GET /api/v1/users/{userId}/user` - Get user by ID
- `PUT /api/v1/users/{userId}/update` - Update user
- `DELETE /api/v1/users/{userId}/delete` - Delete user
- `POST /api/v1/users/reset-password` - Reset password

### Categories
- `GET /api/v1/categories/all` - Get all categories
- `POST /api/v1/categories/add` - Add category (Admin)
- `GET /api/v1/categories/category/{categoryId}` - Get category by ID
- `PUT /api/v1/categories/category/{categoryId}/update` - Update category (Admin)
- `DELETE /api/v1/categories/category/{categoryId}/delete` - Delete category (Admin)

### Products
- `GET /api/v1/products/all` - Get all products
- `GET /api/v1/products/product/{productId}/product` - Get product by ID
- `POST /api/v1/products/add` - Add product (Admin)
- `PUT /api/v1/products/product/{productId}/update` - Update product (Admin)
- `DELETE /api/v1/products/product/{productId}/delete` - Delete product (Admin)
- `GET /api/v1/products/products/by/brand-and-name` - Search by brand and name
- `GET /api/v1/products/products/by/category-and-brand` - Search by category and brand
- `GET /api/v1/products/product/by-brand` - Search by brand
- `GET /api/v1/products/product/{category}/all/products` - Search by category

### Cart
- `POST /api/v1/cartItems/item/add` - Add item to cart
- `DELETE /api/v1/cartItems/cart/{cartId}/item/{itemId}/remove` - Remove item from cart
- `PUT /api/v1/cartItems/cart/{cartId}/item/{itemId}/update` - Update item quantity

### Checkout
- `POST /api/v1/checkout/create-session` - Create Stripe checkout session

### Orders
- `POST /api/v1/orders/order` - Place order
- `GET /api/v1/orders/{orderId}/order` - Get order by ID
- `GET /api/v1/orders/{userId}/orders` - Get orders by user ID

## Error Handling

### Standard Error Response

```json
{
  "message": "Error message",
  "data": null
}
```

### Common Exceptions

- `ResourceNotFoundException` - Entity not found (404)
- `AlreadyExistException` - Duplicate resource (409)
- `InvalidPasswordException` - Wrong password (400)
- `JwtAuthenticationException` - Invalid token (401)
- `ProductNotPresentException` - Product unavailable (404)

## Security

### Best Practices Implemented

1. **JWT Authentication**
   - Token-based authentication
   - Secure secret key (32+ characters)
   - Token expiration
   - Claim-based authorization

2. **Input Validation**
   - Null and empty string checks
   - Email format validation
   - Price and quantity validation

3. **Data Constraints**
   - Unique email constraint
   - Cascade delete rules
   - Foreign key relationships

### Security Recommendations

- Use HTTPS in production
- Store sensitive configuration in environment variables
- Implement rate limiting
- Use API key management for Stripe
- Enable SQL Server encryption
- Implement request logging and monitoring
- Use role-based authorization

## Logging

### Logging Levels

- **Information:** Normal application flow
- **Warning:** Potential issues (user not found, etc.)
- **Error:** Exceptions and failures
- **Debug:** Detailed diagnostic information (development only)

### Access Logs

Logs are written to:
- Console (development)
- File (configurable)
- Event Viewer (Windows)

## Performance Optimization

### Database Indexes

- Email (unique index)
- Product name and brand
- Order status and dates
- Foreign key columns

### Query Optimization

- Use `.Include()` for eager loading
- Use `.AsNoTracking()` for read-only queries
- Implement pagination for large datasets

## Future Enhancements

- [ ] Email notifications for orders
- [ ] Product reviews and ratings
- [ ] Wishlist functionality
- [ ] Inventory management dashboard
- [ ] Advanced analytics and reporting
- [ ] Multi-language support
- [ ] Product images/media storage
- [ ] Coupon and discount management
- [ ] Shipping integration
- [ ] Admin dashboard
