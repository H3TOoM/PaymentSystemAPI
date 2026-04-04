# 🏦 Payment System API

> A modern, clean-architecture Digital Wallet & Payment System built with .NET 10, ASP.NET Core, CQRS, MediatR, Entity Framework Core, JWT Authentication, and Scalar API documentation.

---

## 📑 Table of Contents (Arabic/English)

- [🌟 Features (المميزات)](#-features-المميزات)
- [🏗️ Architecture (الهيكل)](#️-architecture-الهيكل)
- [🛠️ Tech Stack (التقنيات)](#️-tech-stack-التقنيات)
- [🚀 Quick Start (التشغيل السريع)](#-quick-start-التشغيل-السريع)
- [📁 Project Structure (هيكل المشروع)](#-project-structure-هيكل-المشروع)
- [🔐 Security & Auth (الأمان والمصادقة)](#-security--auth-الأمان-والمصادقة)
- [📚 API Documentation (توثيق الـ API)](#-api-documentation-توثيق-ال--api)
- [🧪 Testing & Examples (الاختبارات والأمثلة)](#️-testing--examples-الاختبارات-والأمثلة)
- [🚀 Deployment (النشر)](#-deployment-النشر)
- [📝 Best Practices (أفضل الممارسات)](#-best-practices-أفضل-الممارسات)
- [🤝 Contributing (المساهمة)](#-contributing-المساهمة)

---

## 🌟 Features (المميزات)

### ✅ Core Features
- **User Registration & Login** with secure password hashing
- **JWT Bearer Authentication** with configurable policies
- **Digital Wallets** with multi-currency support
- **Deposits & Withdrawals** with audit logging
- **P2P Transfers** with transaction history
- **Comprehensive Audit Trail** for all operations

### 🛡️ Production-Ready Features
- **Clean Architecture** (Domain, Application, Infrastructure, API)
- **CQRS with MediatR** for scalable command/query separation
- **Global Exception Handling** with standardized JSON responses
- **Request/Response Logging** with correlation IDs
- **Rate Limiting** (Token bucket algorithm)
- **API Versioning** (`/api/v1/`) with multiple readers
- **Health Checks** endpoint
- **Scalar API Documentation** (modern alternative to Swagger)
- **FluentValidation** for request validation
- **Entity Framework Core** with SQL Server and migrations

---

## 🏗️ Architecture (الهيكل)

```
PaymentSystem.API/          # 🌐 API Layer (Controllers, Middleware, DTOs)
├── Controllers/           # Thin controllers using MediatR
├── Middleware/            # Correlation ID, Logging, Rate Limiting, Exception Handling
├── DTOs/                 # Request/Response DTOs
└── Common/               # Shared utilities (ApiResponse)

PaymentSystem.Application/  # 🧠 Application Layer (CQRS, Handlers, Services)
├── Features/             # Organized by domain feature
│   ├── Authentication/   # Register/Login commands
│   ├── Wallets/          # Wallet commands/queries
│   └── Transactions/     # Transfer commands/queries
├── Services/             # JWT Token Service
└── Common/               # Behaviors, Extensions, Exceptions

PaymentSystem.Domain/       # 💎 Domain Layer (Entities, Interfaces, Enums)
├── Entities/             # User, Wallet, Transaction, AuditLog
├── Interfaces/           # Repository contracts
├── Enums/                # Domain enums
└── Common/               # Guard extensions

PaymentSystem.Infrastructure/ # 🗄️ Infrastructure Layer (Data, Repositories)
├── Data/                 # EF Core DbContext, Migrations
├── Repositories/          # Repository implementations
└── DependencyInjection/   # Service registration
```

---

## 🛠️ Tech Stack (التقنيات)

| Layer | Technology |
|-------|------------|
| **API** | ASP.NET Core 10, Scalar, JWT Bearer, Rate Limiting |
| **Application** | MediatR, FluentValidation, CQRS |
| **Domain** | .NET 10, Clean Architecture, DDD patterns |
| **Infrastructure** | EF Core, SQL Server, Repository/UoW |
| **DevOps** | Health Checks, Structured Logging, Correlation IDs |

---

## 🚀 Quick Start (التشغيل السريع)

### Prerequisites
- .NET 10 SDK
- SQL Server (local or Azure)
- Git

### Steps
```bash
# Clone
git clone <repo-url>
cd "Payment System API"

# Restore & Build
dotnet restore
dotnet build

# Configure Database (update appsettings.json connection string)
# Then apply migrations
dotnet ef database update --project PaymentSystem.Infrastructure

# Run
dotnet run --project PaymentSystem.API
```

### Access Points
- **API**: `https://localhost:7xxx/api/v1/`
- **Scalar Docs**: `https://localhost:7xxx/` (dev only)
- **Health Check**: `https://localhost:7xxx/health`

---

## 📁 Project Structure (هيكل المشروع)

### 🌐 API Layer (`PaymentSystem.API`)
- **Controllers**: Auth, Wallets, Transactions, Health
- **Middleware**: CorrelationId, RequestLogging, RateLimiting, GlobalExceptionHandling
- **DTOs**: Request/Response models (no domain exposure)
- **Program.cs**: Pipeline, DI, versioning, auth, Scalar setup

### 🧠 Application Layer (`PaymentSystem.Application`)
- **Features**: Organized by domain (Auth, Wallets, Transactions)
- **Commands/Queries**: CQRS patterns with MediatR
- **Handlers**: Business logic orchestration
- **Services**: JWT Token generation
- **Validation**: FluentValidation rules

### 💎 Domain Layer (`PaymentSystem.Domain`)
- **Entities**: User, Wallet, Transaction, AuditLog, TransactionLog
- **Interfaces**: Repository and Unit of Work contracts
- **Enums**: TransactionType, etc.
- **Common**: Guard clauses, domain primitives

### 🗄️ Infrastructure Layer (`PaymentSystem.Infrastructure`)
- **Data**: AppDbContext, EF Core configuration
- **Repositories**: EF-based implementations
- **Migrations**: Database schema versioning
- **DI**: Infrastructure service registration

---

## 🔐 Security & Auth (الأمان والمصادقة)

### JWT Configuration
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKey...32+chars",
    "Issuer": "PaymentSystemAPI",
    "Audience": "PaymentSystemClients",
    "ExpirationMinutes": 60
  }
}
```

### Security Features
- **Password Hashing**: ASP.NET Core `PasswordHasher`
- **JWT Validation**: Issuer, Audience, Lifetime, Signature
- **Rate Limiting**: 100 tokens, 20/10s refill
- **Correlation IDs**: Trace requests across logs
- **Input Validation**: FluentValidation with detailed errors
- **Secure Headers**: HTTPS only in production

---

## 📚 API Documentation (توثيق الـ API)

### 🌐 Scalar UI (Development)
- Navigate to root URL (`/`) in dev
- JWT Bearer auth supported
- Try endpoints directly from browser
- Export to OpenAPI/Postman

### 📋 Endpoints Summary

#### 🔐 Authentication (`/api/v1/auth`)
- `POST /register` – User registration
- `POST /login` – User login (returns JWT)

#### 💰 Wallets (`/api/v1/wallets`) – `[Authorize]`
- `GET /{userId}` – Get wallet details
- `POST /deposit` – Deposit funds
- `POST /withdraw` – Withdraw funds

#### 🔄 Transactions (`/api/v1/transactions`) – `[Authorize]`
- `POST /transfer` – P2P transfer
- `GET /{userId}` – Transaction history

#### 🏥 Health (`/api/v1/health`)
- `GET /` – Service health status

### 📦 Response Format
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... },
  "correlationId": "..."
}
```

### 🚨 Error Format
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": ["Email is required.", "Password too short."],
  "correlationId": "..."
}
```

---

## 🧪 Testing & Examples (الاختبارات والأمثلة)

### Postman Collection
```json
{
  "info": { "name": "Payment System API" },
  "variable": [
    { "key": "baseUrl", "value": "https://localhost:7xxx/api/v1" },
    { "key": "token", "value": "" }
  ],
  "item": [
    {
      "name": "Register",
      "request": {
        "method": "POST",
        "header": [{ "key": "Content-Type", "value": "application/json" }],
        "body": {
          "mode": "raw",
          "raw": "{\"name\":\"John Doe\",\"email\":\"john@example.com\",\"password\":\"P@ssw0rd!\"}"
        },
        "url": "{{baseUrl}}/auth/register"
      }
    },
    {
      "name": "Login",
      "request": {
        "method": "POST",
        "header": [{ "key": "Content-Type", "value": "application/json" }],
        "body": {
          "mode": "raw",
          "raw": "{\"email\":\"john@example.com\",\"password\":\"P@ssw0rd!\"}"
        },
        "url": "{{baseUrl}}/auth/login"
      },
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": ["if (pm.response.code === 200) { const json = pm.response.json(); pm.collectionVariables.set('token', json.data.token); }"]
          }
        }
      ]
    }
  ]
}
```

---

## 🚀 Deployment (النشر)

### Production Checklist
- [ ] Move `Jwt.SecretKey` to Azure Key Vault or environment variables
- [ ] Configure production connection string
- [ ] Enable HTTPS with valid certificates
- [ ] Set up Application Insights/structured logging
- [ ] Configure rate limits per environment
- [ ] Run database migrations in CI/CD pipeline
- [ ] Enable health checks behind load balancer
- [ ] Set up API versioning strategy for clients

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PaymentSystem.API/PaymentSystem.API.csproj", "PaymentSystem.API/"]
COPY ["PaymentSystem.Application/PaymentSystem.Application.csproj", "PaymentSystem.Application/"]
COPY ["PaymentSystem.Domain/PaymentSystem.Domain.csproj", "PaymentSystem.Domain/"]
COPY ["PaymentSystem.Infrastructure/PaymentSystem.Infrastructure.csproj", "PaymentSystem.Infrastructure/"]
RUN dotnet restore "PaymentSystem.API/PaymentSystem.API.csproj"
COPY . .
WORKDIR "/src/PaymentSystem.API"
RUN dotnet build "PaymentSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentSystem.API.dll"]
```

---

## 📝 Best Practices (أفضل الممارسات)

### ✅ Code Quality
- **Clean Architecture**: Dependency inversion, separation of concerns
- **CQRS**: Separate read/write models for scalability
- **Async/Await**: Non-blocking I/O throughout
- **CancellationToken**: Proper cancellation support
- **Guard Clauses**: Domain invariants protection

### 🛡️ Security
- **Never expose passwords** or hashes in responses/logs
- **Use HTTPS** in all environments
- **Validate all inputs** with FluentValidation
- **Rate limit** public endpoints
- **Correlation IDs** for traceability

### 📊 Observability
- **Structured logging** with correlation IDs
- **Health checks** for load balancers
- **OpenAPI/Scalar** for discoverability
- **Exception handling** with user-friendly messages

### 🚀 Performance
- **EF Core** with proper change tracking
- **Repository/UoW** pattern for transaction boundaries
- **Token bucket** rate limiting
- **Minimal middleware** overhead

---

## 🤝 Contributing (المساهمة)

1. Fork the repo
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### 📜 License
This project is licensed under the MIT License.

---

## 👏 Acknowledgments

- Clean Architecture principles by Jason Taylor
- MediatR for CQRS implementation
- Scalar for modern API docs
- ASP.NET Core team for excellent framework

---
