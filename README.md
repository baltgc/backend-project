# NetChallenge API

A REST API built with .NET 8 that consumes data from the JSONPlaceholder external API, featuring JWT authentication, Clean Architecture, and comprehensive documentation.

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
- Implements JWT (JSON Web Token) authentication
- Follows Clean Architecture principles
- Applies SOLID principles throughout the codebase

## Technologies Used

- **.NET 8** - Framework
- **C#** - Programming language
- **JWT Authentication** - Security
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **HttpClient** - External API consumption

## Project Structure (Clean Architecture)

```
NetChallenge/
├── NetChallenge.API/           # Controllers, middleware, configuration
├── NetChallenge.Application/   # Use cases, interfaces, DTOs
├── NetChallenge.Domain/        # Business entities
├── NetChallenge.Infrastructure/# External services, repositories
└── NetChallenge.Application.Tests/ # Unit tests
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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
Generate a JWT token.

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
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

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
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "NetChallengeAPI",
    "Audience": "NetChallengeAPI",
    "ExpirationMinutes": 60
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
| `Authentication__ValidUsername` | Valid username for login |
| `Authentication__ValidPassword` | Valid password for login |

## Middleware

The API includes a **Global Exception Handler** middleware that:
- Catches all unhandled exceptions
- Logs errors using the built-in `ILogger`
- Returns a clean JSON response instead of exposing stack traces

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
