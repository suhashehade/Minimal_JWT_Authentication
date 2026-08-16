# Minimal ASP.NET Core Web API with JWT Authentication

## Overview

This project is a simple **ASP.NET Core Minimal Web API** that demonstrates how to implement **JWT (JSON Web Token) authentication and authorization**.

The project was created as a learning exercise to understand:

- Minimal APIs in ASP.NET Core
- Dependency Injection
- JWT token generation
- JWT token validation
- JWT Bearer Authentication
- Authorization for protected endpoints
- Testing authenticated and unauthenticated requests

---

## Technologies Used

- .NET 10
- ASP.NET Core Minimal API
- JWT Bearer Authentication
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- Postman for API testing

---

## Project Structure

The main parts of the project are:

```text
Minimal-JWT-Authentication/
│
├── Auth/
│   └── JwtSettings.cs
│
├── JwtTokenGenerator.cs
├── Program.cs
├── appsettings.json
└── README.md
```

---

# 1. JWT Settings

The JWT configuration is stored in `appsettings.json`:

```json
"JwtSettings": {
  "Key": "my-auth-super-secret-key-1234567890",
  "Issuer": "auth",
  "Audience": "audience",
  "ExpiryMinutes": 30
}
```

These settings are represented by the `JwtSettings` class:

```csharp
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}
```

### Purpose of each setting

| Setting | Purpose |
|---|---|
| `Key` | Secret key used to sign and validate the JWT |
| `Issuer` | Identifies who issued the token |
| `Audience` | Identifies who the token is intended for |
| `ExpiryMinutes` | Determines how long the token remains valid |

> For a real production application, the secret key should not be stored directly in `appsettings.json`. A secure secret-management solution should be used instead.

---

# 2. JWT Token Generator

The `JwtTokenGenerator` class is responsible for creating and validating JWT tokens.

It receives `JwtSettings` through `IOptions<JwtSettings>`.

## GenerateToken

The `GenerateToken` method creates a JWT using:

- Username as the `sub` claim
- A unique `jti`
- Expiration time
- Issuer
- Audience
- A symmetric signing key
- HMAC SHA-256 (`HS256`) signing algorithm

The token is created using `JsonWebTokenHandler`.

Conceptually:

```text
Username
   +
JWT Settings
   ↓
Claims + Expiration
   ↓
Signing Credentials
   ↓
JWT Token
```

---

# 3. Token Validation

The `ValidateToken` method validates a JWT using `TokenValidationParameters`.

The following checks are performed:

```text
Signature
Issuer
Audience
Expiration
```

Important validation settings include:

```csharp
ValidateIssuerSigningKey = true
ValidateIssuer = true
ValidateAudience = true
ValidateLifetime = true
```

The same secret key used to create the token is used to validate its signature.

---

# 4. Dependency Injection

`JwtTokenGenerator` is registered as a service:

```csharp
builder.Services.AddScoped<JwtTokenGenerator>();
```

This allows ASP.NET Core to inject `JwtTokenGenerator` into the login endpoint.

For example:

```csharp
app.MapPost("/login", (
    LoginRequest request,
    JwtTokenGenerator tokenGenerator) =>
{
    // ...
});
```

ASP.NET Core automatically provides the registered `JwtTokenGenerator` instance.

---

# 5. JWT Authentication Configuration

JWT Bearer Authentication is configured in `Program.cs`.

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });
```

This tells ASP.NET Core how to validate JWT tokens received in requests.

---

# 6. Authorization

Authorization is enabled with:

```csharp
builder.Services.AddAuthorization();
```

The authentication and authorization middleware are then added:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

The order is important:

```text
Request
  ↓
Authentication
  ↓
Authorization
  ↓
Endpoint
```

Authentication determines **who the user is**.

Authorization determines **whether the user is allowed to access the requested resource**.

---

# 7. Minimal API Endpoints

## Public Endpoint

The root endpoint is public:

```csharp
app.MapGet("/", () => "Hello World!");
```

It does not require authentication.

---

## Login Endpoint

A simple login endpoint was created for testing.

```text
POST /login
```

It receives:

```json
{
  "username": "suha",
  "password": "123"
}
```

For this learning exercise, the credentials are hard-coded:

```text
Username: suha
Password: 123
```

If the credentials are valid, `JwtTokenGenerator.GenerateToken()` is called and the JWT is returned.

If the credentials are invalid, the API returns:

```text
401 Unauthorized
```

The login endpoint is only a simple demonstration. A real application would normally verify the credentials against a database and would never store passwords in plain text.

---

# 8. Protected Endpoint

The `/welcome` endpoint requires authentication:

```csharp
app.MapGet(
    "/welcome",
    () => "Welcome! You are authorized."
)
.RequireAuthorization();
```

This means the client must provide a valid JWT.

Without a valid token:

```text
401 Unauthorized
```

With a valid token:

```text
200 OK
Welcome! You are authorized.
```

---

# 9. Testing the API

The API was tested using Postman.

## Test 1: Public Endpoint

Request:

```http
GET /
```

Expected response:

```text
200 OK
Hello World!
```

---

## Test 2: Protected Endpoint Without Token

Request:

```http
GET /welcome
```

without an `Authorization` header.

Expected response:

```text
401 Unauthorized
```

This confirms that authorization is working.

---

## Test 3: Login

Request:

```http
POST /login
Content-Type: application/json
```

Body:

```json
{
  "username": "suha",
  "password": "123"
}
```

Expected response:

```json
{
  "token": "eyJ..."
}
```

The returned token is the JWT generated by the application.

---

## Test 4: Protected Endpoint With JWT

Copy the JWT returned by `/login`.

Send:

```http
GET /welcome
Authorization: Bearer <JWT>
```

Example:

```http
Authorization: Bearer eyJ...
```

Expected response:

```text
200 OK
Welcome! You are authorized.
```

---

# 10. Authentication Flow

The complete flow of the application is:

```text
Client
  │
  │ POST /login
  │ username + password
  ↓
Login Endpoint
  │
  │ credentials valid?
  ↓
JwtTokenGenerator
  │
  │ GenerateToken()
  ↓
JWT
  │
  │ Authorization: Bearer <JWT>
  ↓
Protected Endpoint
  │
  ↓
JWT Bearer Authentication
  │
  ├── Signature valid?
  ├── Issuer valid?
  ├── Audience valid?
  └── Token not expired?
  │
  ↓
Authorization
  │
  ↓
/welcome
  │
  ↓
Welcome! You are authorized.
```

---

# 11. Important Concepts Learned

### Minimal API

A lightweight way to create HTTP endpoints in ASP.NET Core without using traditional controllers.

### Dependency Injection

ASP.NET Core can create and provide registered services automatically when an endpoint or class needs them.

### JWT

A signed token that can carry claims and can be used to represent an authenticated user.

### Authentication

Determines whether the supplied credentials/token represent a valid authenticated user.

### Authorization

Determines whether an authenticated user has permission to access a protected endpoint.

### JWT Bearer Authentication

ASP.NET Core reads a JWT from the HTTP `Authorization` header:

```http
Authorization: Bearer <token>
```

and validates it according to the configured validation parameters.

---

# Conclusion

This project demonstrates a basic JWT authentication flow using ASP.NET Core Minimal API.

The application can:

1. Create JWT tokens.
2. Validate JWT tokens.
3. Authenticate requests using JWT Bearer Authentication.
4. Protect endpoints using authorization.
5. Return `401 Unauthorized` when authentication is missing or invalid.
6. Allow access to protected endpoints when a valid JWT is supplied.

The implementation is intentionally simple and is intended for learning the fundamentals of ASP.NET Core and JWT authentication.
