# NetChallenge API

A REST API built with **.NET 9** that consumes data from the **JSONPlaceholder** external API, featuring **JWT auth + refresh tokens**, **DB-backed cache**, and production-grade middleware (correlation IDs, ProblemDetails, auditing).

## Quick Start

```bash
# Run with Docker (recommended)
docker-compose up --build

# Test login (get JWT token)
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Use token to get users
curl http://localhost:8080/api/users \
  -H "Authorization: Bearer <your-token>"
```

Swagger UI: http://localhost:8080

## Description

This project implements a secure REST API that:
- Consumes user data from [JSONPlaceholder API](https://jsonplaceholder.typicode.com/)
- Implements JWT (JSON Web Token) authentication **with refresh-token rotation**
- Follows Clean Architecture principles
- Applies SOLID principles throughout the codebase

## Features

- **JWT auth + refresh tokens**
  - Access tokens for protected endpoints
  - Refresh token rotation + revocation (`/api/auth/refresh`, `/api/auth/logout`)
- **Postgres persistence (EF Core)**
  - `user_accounts`, `refresh_tokens`, `cache_entries`, `audit_events`
  - Dev-only auto-migrate + seed admin user on startup
- **DB-backed cache for external API calls**
  - Persists JSON payloads in `cache_entries`
  - TTL configurable via `Cache:UsersTtlSeconds`
- **Correlation IDs**
  - Request/response header: `X-Correlation-ID`
  - Added to logs and returned in ProblemDetails
- **RFC7807 ProblemDetails error responses**
  - `application/problem+json`
  - Includes `correlationId` extension
- **Audit trail**
  - Persists one row per request in `audit_events` (best-effort; never breaks the request)
- **Resilience for JSONPlaceholder**
  - HttpClient policies: retry, timeout, circuit breaker
- **Health checks**
  - `GET /health` (includes DB check)
- **Test coverage**
  - Unit tests (`NetChallenge.Application.Tests`)
  - API integration tests (`NetChallenge.API.Tests`) using `WebApplicationFactory` + in-memory SQLite + fake JsonPlaceholder

## Technologies Used

- **.NET 9** - Framework
- **C#** - Programming language
- **JWT Authentication** - Security
- **Refresh Tokens** - Session continuation + rotation
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization
- **PostgreSQL** - Database
- **EF Core** - ORM
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **HttpClient** - External API consumption
- **Polly** - Resilience policies (retry/timeout/circuit breaker)

## Project Structure (Clean Architecture)

```
NetChallenge/
├── NetChallenge.API/           # Controllers, middleware, configuration
├── NetChallenge.Application/   # Use cases, interfaces, DTOs
├── NetChallenge.Domain/        # Business entities
├── NetChallenge.Infrastructure/# External services, repositories
└── NetChallenge.Application.Tests/ # Unit tests
└── NetChallenge.API.Tests/     # API integration tests
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerized execution)

## Installation & Running

### Option 1: Run Locally

```bash
# Clone the repository
git clone <repository-url>
cd netchallenge

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project NetChallenge.API
```

The API will be available at `http://localhost:5108`

### Option 2: Run with Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Or build and run manually
docker build -t netchallenge-api .
docker run -p 8080:8080 netchallenge-api
```

The API will be available at `http://localhost:8080`

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with Docker Compose
docker-compose --profile test run tests
```

## API Documentation

When running the application, Swagger UI is available at:
- Local: `http://localhost:5108/`
- Docker: `http://localhost:8080/`

## API Endpoints

### Authentication

#### POST /api/auth/login
Generate a JWT access token and refresh token.

**Request:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "refreshToken": "base64url...",
  "refreshTokenExpiresAt": "2024-01-08T12:00:00Z"
}
```

#### POST /api/auth/refresh
Rotate the refresh token and return a new access token.

**Request:**
```json
{
  "refreshToken": "base64url..."
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "refreshToken": "base64url...",
  "refreshTokenExpiresAt": "2024-01-08T12:00:00Z"
}
```

#### POST /api/auth/logout
Revoke a refresh token (best-effort, always returns 204).

**Request:**
```json
{
  "refreshToken": "base64url..."
}
```

**Response:** `204 No Content`

### Users (Protected - Requires JWT Token)

#### GET /api/users
Get all users from JSONPlaceholder API.

**Headers:**
```
Authorization: Bearer <your-token>
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Leanne Graham",
    "username": "Bret",
    "email": "Sincere@april.biz",
    "phone": "1-770-736-8031 x56442",
    "website": "hildegard.org"
  }
]
```

#### GET /api/users/{id}
Get a specific user by ID.

**Headers:**
```
Authorization: Bearer <your-token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Leanne Graham",
  "username": "Bret",
  "email": "Sincere@april.biz",
  "phone": "1-770-736-8031 x56442",
  "website": "hildegard.org"
}
```

## Error Responses

| Status Code | Description |
|-------------|-------------|
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Invalid credentials or missing/invalid token |
| 404 | Not Found - User not found |
| 503 | Upstream Service Failure - JSONPlaceholder is unavailable |

All errors are returned as **ProblemDetails** (`application/problem+json`) and include a `correlationId` field.

## Test Credentials

| Username | Password |
|----------|----------|
| admin    | admin123 |

## Authentication Flow

1. Call `POST /api/auth/login` with valid credentials
2. Copy the `token` from the response
3. Include the token in the `Authorization` header for protected endpoints:
   ```
   Authorization: Bearer <your-token>
   ```

## Using Swagger UI for Authentication

1. Open Swagger UI at the root URL
2. Click the "Authorize" button
3. Enter your JWT token (without "Bearer" prefix)
4. Click "Authorize"
5. Now all protected endpoints can be tested

## API Collection (Bruno)

The project includes a [Bruno](https://www.usebruno.com/) collection in the `/bruno` folder for testing the API endpoints.

### Using Bruno

1. Download and install [Bruno](https://www.usebruno.com/)
2. Open Bruno and click "Open Collection"
3. Navigate to the `bruno` folder in this project
4. Select the environment (local or docker)
5. Run the requests in order:
   - **Login** - Gets JWT token (auto-saved to `token` variable)
   - **Get All Users** - Lists all users (requires token)
   - **Get User By Id** - Gets user by ID (requires token)

### Environments

| Environment | Base URL |
|-------------|----------|
| local | `http://localhost:5108` |
| docker | `http://localhost:8080` |

## Configuration

Configuration is managed through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=netchallenge;Username=netchallenge;Password=netchallenge"
  },
  "Cache": {
    "UsersTtlSeconds": 60
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "NetChallengeAPI",
    "Audience": "NetChallengeAPI",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Authentication": {
    "ValidUsername": "admin",
    "ValidPassword": "admin123"
  },
  "JsonPlaceholder": {
    "BaseUrl": "https://jsonplaceholder.typicode.com"
  }
}
```

## Environment Variables

For production, override settings using environment variables:

| Variable | Description |
|----------|-------------|
| `Jwt__SecretKey` | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | Token issuer |
| `Jwt__Audience` | Token audience |
| `Jwt__ExpirationMinutes` | Token validity in minutes |
| `Jwt__RefreshTokenExpirationDays` | Refresh token validity in days |
| `Authentication__ValidUsername` | Valid username for login |
| `Authentication__ValidPassword` | Valid password for login |
| `ConnectionStrings__Postgres` | Postgres connection string |
| `Cache__UsersTtlSeconds` | Cache TTL for users (seconds) |

## Middleware

The API includes:

- **CorrelationIdMiddleware**: sets `X-Correlation-ID` and adds it to logging scope
- **ExceptionHandlerMiddleware**: returns RFC7807 ProblemDetails (`application/problem+json`) with `correlationId`
- **AuditMiddleware**: writes one `audit_events` row per request (best-effort)

## Architecture & SOLID Principles

### Clean Architecture Layers

- **Domain**: Contains `User` entity - pure business objects with no dependencies
- **Application**: Contains use cases (`LoginUseCase`, `GetUsersUseCase`, `GetUserByIdUseCase`), interfaces (`IAuthService`, `IUserService`), and DTOs
- **Infrastructure**: Implements interfaces (`AuthService`, `UserService`) and external API clients (`JsonPlaceholderClient`)
- **API**: Controllers, middleware, and DI configuration

### SOLID Principles Applied

- **S**ingle Responsibility: Each class has one reason to change (e.g., separate use cases for each operation)
- **O**pen/Closed: Use cases are open for extension through interfaces
- **L**iskov Substitution: Services can be replaced with any implementation of their interfaces
- **I**nterface Segregation: Small, focused interfaces (`IAuthService`, `IUserService`)
- **D**ependency Inversion: High-level modules depend on abstractions (interfaces), not concrete implementations
