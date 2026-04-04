# 🏗️ Architecture Guide & Flow Diagrams

## 📋 Architecture Overview

The Payment System API follows **Clean Architecture** principles with clear separation of concerns across four distinct layers. Each layer has specific responsibilities and dependency flow.

---

## 🏛️ Clean Architecture Layers

```mermaid
graph TB
    subgraph "Presentation Layer"
        A[API Controllers]
        B[Middleware]
        C[DTOs]
    end
    
    subgraph "Application Layer"
        D[CQRS Commands]
        E[CQRS Queries]
        F[Handlers]
        G[Validators]
        H[Services]
    end
    
    subgraph "Domain Layer"
        I[Entities]
        J[Interfaces]
        K[Enums]
        L[Common]
    end
    
    subgraph "Infrastructure Layer"
        M[EF Core]
        N[Repositories]
        O[External Services]
    end
    
    A --> D
    A --> E
    B --> F
    C --> D
    C --> E
    
    D --> F
    E --> F
    F --> H
    G --> F
    
    F --> I
    H --> J
    I --> J
    
    N --> M
    N --> J
    O --> J
    
    F --> N
```

---

## 🔄 Request Flow Architecture

### Authentication Flow
```mermaid
sequenceDiagram
    participant Client
    participant API
    participant AuthHandler
    participant UserService
    participant TokenService
    participant Database
    
    Client->>API: POST /api/v1/auth/login
    API->>API: Correlation ID Middleware
    API->>API: Request Logging
    API->>API: Rate Limiting
    API->>API: Validation
    API->>AuthHandler: LoginUserCommand
    AuthHandler->>UserService: GetByEmailAsync
    UserService->>Database: SELECT User WHERE Email = ?
    Database-->>UserService: User Entity
    UserService-->>AuthHandler: User
    AuthHandler->>AuthHandler: Verify Password Hash
    AuthHandler->>TokenService: GenerateToken
    TokenService-->>AuthHandler: JWT Token
    AuthHandler-->>API: LoginUserResult
    API-->>Client: 200 OK + JWT
```

### Transaction Flow
```mermaid
sequenceDiagram
    participant Client
    participant API
    participant TransferHandler
    participant UserService
    participant WalletService
    participant TransactionService
    participant Database
    participant AuditService
    
    Client->>API: POST /api/v1/transactions/transfer
    API->>API: Auth Middleware (JWT Validation)
    API->>TransferHandler: TransferMoneyCommand
    TransferHandler->>UserService: GetByIdAsync (Sender)
    UserService->>Database: SELECT User WHERE Id = ?
    Database-->>UserService: Sender User
    UserService-->>TransferHandler: Sender
    
    TransferHandler->>UserService: GetByIdAsync (Receiver)
    UserService->>Database: SELECT User WHERE Id = ?
    Database-->>UserService: Receiver User
    UserService-->>TransferHandler: Receiver
    
    TransferHandler->>WalletService: GetByUserIdAsync (Sender)
    WalletService->>Database: SELECT Wallet WHERE UserId = ?
    Database-->>WalletService: Sender Wallet
    WalletService-->>TransferHandler: Sender Wallet
    
    TransferHandler->>WalletService: GetByUserIdAsync (Receiver)
    WalletService->>Database: SELECT Wallet WHERE UserId = ?
    Database-->>WalletService: Receiver Wallet
    WalletService-->>TransferHandler: Receiver Wallet
    
    TransferHandler->>TransferHandler: Validate Business Rules
    
    TransferHandler->>TransactionService: Create Transaction
    TransactionService->>Database: INSERT Transaction
    Database-->>TransactionService: Transaction
    
    TransferHandler->>WalletService: Withdraw (Sender)
    WalletService->>Database: UPDATE Wallet Balance
    TransferHandler->>WalletService: Deposit (Receiver)
    WalletService->>Database: UPDATE Wallet Balance
    
    TransferHandler->>AuditService: Add Audit Log (Sender)
    AuditService->>Database: INSERT AuditLog
    TransferHandler->>AuditService: Add Audit Log (Receiver)
    AuditService->>Database: INSERT AuditLog
    
    TransferHandler->>Database: UnitOfWork.SaveChanges()
    Database-->>TransferHandler: Success
    
    TransferHandler-->>API: TransferMoneyResult
    API-->>Client: 200 OK + Transaction Details
```

---

## 🏗️ Layer Responsibilities

### 🌐 API Layer (Presentation)
**Purpose**: HTTP request handling and response formatting

**Components**:
- **Controllers**: Thin endpoints using MediatR
- **Middleware**: Cross-cutting concerns (auth, logging, rate limiting)
- **DTOs**: Data transfer objects (no domain exposure)
- **Program.cs**: Application configuration and pipeline

**Key Rules**:
- No business logic
- Always use MediatR for operations
- Standardized response format
- Correlation ID propagation

### 🧠 Application Layer (Use Cases)
**Purpose**: Business logic orchestration and application services

**Components**:
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, List, Search)
- **Handlers**: Command/query processing logic
- **Validators**: Input validation rules
- **Services**: Application services (JWT, etc.)

**Key Rules**:
- CQRS pattern implementation
- Use repositories for data access
- Dependency inversion (depends on abstractions)
- Transaction management via Unit of Work

### 💎 Domain Layer (Business Rules)
**Purpose**: Core business entities and domain logic

**Components**:
- **Entities**: Business objects with behavior
- **Interfaces**: Repository contracts
- **Enums**: Domain-specific enumerations
- **Common**: Shared domain utilities

**Key Rules**:
- No external dependencies
- Rich domain models with behavior
- Domain invariants via guard clauses
- Pure business logic

### 🗄️ Infrastructure Layer (Data)
**Purpose**: Data persistence and external integrations

**Components**:
- **DbContext**: EF Core data access
- **Repositories**: Data access implementations
- **Migrations**: Database schema versioning
- **External Services**: Third-party integrations

**Key Rules**:
- Implements domain interfaces
- Framework-specific code only
- Database optimizations
- External service adapters

---

## 🔧 Dependency Flow

```mermaid
graph LR
    subgraph "Outer Layer"
        API[API Layer]
    end
    
    subgraph "Middle Layer"
        APP[Application Layer]
    end
    
    subgraph "Inner Layer"
        DOMAIN[Domain Layer]
    end
    
    subgraph "External Layer"
        INFRA[Infrastructure Layer]
    end
    
    API --> APP
    APP --> DOMAIN
    INFRA --> DOMAIN
    APP --> INFRA
```

**Dependency Rules**:
- Dependencies point inward only
- API depends on Application
- Application depends on Domain and Infrastructure
- Infrastructure depends on Domain
- Domain has no dependencies

---

## 🗄️ Database Architecture

### Entity Relationship Diagram
```mermaid
erDiagram
    User ||--o{ Wallet : has
    User ||--o{ AuditLog : creates
    Wallet ||--o{ Transaction : contains
    Transaction ||--o{ TransactionLog : has
    
    User {
        Guid Id PK
        string FullName
        string Email
        string PasswordHash
        bool IsActive
        DateTime CreatedAtUtc
    }
    
    Wallet {
        Guid Id PK
        Guid UserId FK
        string Currency
        decimal Balance
        DateTime CreatedAtUtc
        byte[] RowVersion
    }
    
    Transaction {
        Guid Id PK
        Guid WalletId FK
        string ReferenceId
        string Type
        decimal Amount
        string Status
        DateTime CreatedAtUtc
        byte[] RowVersion
    }
    
    TransactionLog {
        Guid Id PK
        Guid TransactionId FK
        string Action
        string Details
        DateTime CreatedAtUtc
    }
    
    AuditLog {
        Guid Id PK
        Guid UserId FK
        string Action
        string TargetType
        string Details
        DateTime CreatedAtUtc
    }
```

### Database Design Principles
- **Normalization**: 3NF with proper relationships
- **Concurrency**: RowVersion for optimistic concurrency
- **Indexing**: Strategic indexes for performance
- **Auditing**: Complete audit trail for compliance

---

## 🔐 Security Architecture

### Authentication Flow
```mermaid
graph TB
    subgraph "Client"
        A[Web/Mobile App]
    end
    
    subgraph "API Gateway"
        B[Load Balancer]
        C[SSL Termination]
    end
    
    subgraph "API Layer"
        D[Authentication Middleware]
        E[JWT Validation]
        F[Authorization]
    end
    
    subgraph "Application"
        G[Login Handler]
        H[Password Verification]
        I[Token Generation]
    end
    
    subgraph "Infrastructure"
        J[User Repository]
        K[Database]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> F
    F --> G
    G --> H
    H --> J
    J --> K
    G --> I
    I --> D
```

### Security Layers
1. **Network Layer**: SSL/TLS, Firewall rules
2. **API Layer**: Authentication, Authorization, Rate Limiting
3. **Application Layer**: Input validation, Business rules
4. **Data Layer**: Encryption, Access controls

---

## 📊 Performance Architecture

### Caching Strategy
```mermaid
graph TB
    subgraph "Client"
        A[Web/Mobile App]
    end
    
    subgraph "API Layer"
        B[Output Cache]
        C[Response Cache]
    end
    
    subgraph "Application"
        D[In-Memory Cache]
        E[Distributed Cache]
    end
    
    subgraph "Infrastructure"
        F[Database]
        G[Redis Cache]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> G
    D --> F
    E --> F
```

### Performance Optimizations
- **Database**: Connection pooling, query optimization
- **Caching**: Multi-level caching strategy
- **Async**: Non-blocking I/O throughout
- **Rate Limiting**: Prevent abuse and overload

---

## 🔄 Scalability Architecture

### Horizontal Scaling
```mermaid
graph TB
    subgraph "Load Balancer"
        LB[Application Gateway]
    end
    
    subgraph "API Instances"
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance N]
    end
    
    subgraph "Shared Infrastructure"
        DB[(Database)]
        CACHE[(Redis Cache)]
        QUEUE[Message Queue]
    end
    
    LB --> API1
    LB --> API2
    LB --> API3
    
    API1 --> DB
    API2 --> DB
    API3 --> DB
    
    API1 --> CACHE
    API2 --> CACHE
    API3 --> CACHE
    
    API1 --> QUEUE
    API2 --> QUEUE
    API3 --> QUEUE
```

### Scaling Strategies
- **Stateless API**: Easy horizontal scaling
- **Database Sharding**: Distribute data load
- **Caching Layer**: Reduce database load
- **Message Queue**: Async processing for heavy operations

---

## 🧪 Testing Architecture

### Test Pyramid
```mermaid
graph TB
    subgraph "E2E Tests"
        E2E[End-to-End Tests]
    end
    
    subgraph "Integration Tests"
        INT[API Integration Tests]
        DB[Database Tests]
    end
    
    subgraph "Unit Tests"
        UNIT[Domain Tests]
        HANDLER[Handler Tests]
        SERVICE[Service Tests]
    end
    
    E2E --> INT
    INT --> UNIT
```

### Testing Strategy
- **Unit Tests**: Fast, isolated tests for business logic
- **Integration Tests**: Database and external service integration
- **E2E Tests**: Full request/response cycles
- **Performance Tests**: Load and stress testing

---

## 📝 Monitoring Architecture

### Observability Stack
```mermaid
graph TB
    subgraph "Application"
        A[Structured Logging]
        B[Metrics Collection]
        C[Health Checks]
    end
    
    subgraph "Collection"
        D[Application Insights]
        E[Prometheus]
        F[Health Monitor]
    end
    
    subgraph "Visualization"
        G[Dashboards]
        H[Alerts]
        I[Reports]
    end
    
    A --> D
    B --> E
    C --> F
    
    D --> G
    E --> G
    F --> G
    
    G --> H
    G --> I
```

### Monitoring Components
- **Logging**: Structured logs with correlation IDs
- **Metrics**: Performance and business metrics
- **Health Checks**: Service availability monitoring
- **Alerting**: Proactive issue notification

---

## 🔧 Configuration Architecture

### Configuration Hierarchy
```mermaid
graph TB
    subgraph "Configuration Sources"
        A[appsettings.json]
        B[appsettings.{Environment}.json]
        C[Environment Variables]
        D[Azure Key Vault]
        E[Command Line Arguments]
    end
    
    subgraph "Configuration Provider"
        F[.NET Configuration]
    end
    
    subgraph "Application"
        G[IOptions<T>]
        H[Settings Classes]
    end
    
    A --> F
    B --> F
    C --> F
    D --> F
    E --> F
    
    F --> G
    G --> H
```

### Configuration Management
- **Development**: Local configuration files
- **Production**: Secure secret management
- **Environment-specific**: Override base settings
- **Runtime**: Hot reload where applicable

---

## 🚀 Deployment Architecture

### Container Deployment
```mermaid
graph TB
    subgraph "CI/CD Pipeline"
        A[Build]
        B[Test]
        C[Security Scan]
        D[Package]
    end
    
    subgraph "Container Registry"
        E[Docker Hub/Azure Container Registry]
    end
    
    subgraph "Orchestration"
        F[Kubernetes/Docker Compose]
    end
    
    subgraph "Infrastructure"
        G[Load Balancer]
        H[API Containers]
        I[Database]
        J[Cache]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> F
    F --> G
    G --> H
    H --> I
    H --> J
```

### Deployment Strategies
- **Blue-Green**: Zero-downtime deployments
- **Canary**: Gradual rollout with monitoring
- **Rolling**: Update instances incrementally
- **Rollback**: Quick revert capability

---

## 📚 Architecture Decision Records (ADRs)

### ADR-001: Clean Architecture Implementation
**Status**: Accepted
**Decision**: Implement Clean Architecture with clear layer separation
**Consequences**: 
- ✅ Maintainable codebase
- ✅ Testable components
- ✅ Flexible dependencies
- ❌ Increased complexity
- ❌ More boilerplate code

### ADR-002: CQRS Pattern
**Status**: Accepted
**Decision**: Use CQRS with MediatR for command/query separation
**Consequences**:
- ✅ Scalable read/write operations
- ✅ Clear intent separation
- ✅ Optimized queries
- ❌ Increased complexity
- ❌ More code to maintain

### ADR-003: JWT Authentication
**Status**: Accepted
**Decision**: Use JWT Bearer tokens for authentication
**Consequences**:
- ✅ Stateless authentication
- ✅ Cross-platform support
- ✅ Scalable solution
- ❌ Token revocation complexity
- ❌ Larger request headers

### ADR-004: Entity Framework Core
**Status**: Accepted
**Decision**: Use EF Core for data access
**Consequences**:
- ✅ Rapid development
- ✅ Database abstraction
- ✅ Migration support
- ❌ Performance overhead
- ❌ Limited control over SQL

---

## 🔄 Evolution Path

### Phase 1: Core Features ✅
- User authentication
- Basic wallet operations
- Simple transfers
- Audit logging

### Phase 2: Advanced Features 🚧
- Multi-currency support
- Transaction scheduling
- Advanced reporting
- Performance optimizations

### Phase 3: Enterprise Features 📋
- Microservices architecture
- Event sourcing
- CQRS with separate read models
- Advanced security features

### Phase 4: Scale & Optimization 📋
- Database sharding
- Distributed caching
- Global deployment
- AI-powered fraud detection

---

## 📋 Architecture Checklist

### Design Principles
- [ ] Single Responsibility Principle
- [ ] Open/Closed Principle
- [ ] Liskov Substitution Principle
- [ ] Interface Segregation Principle
- [ ] Dependency Inversion Principle

### Quality Attributes
- [ ] Performance requirements met
- [ ] Security measures implemented
- [ ] Scalability considerations
- [ ] Maintainability ensured
- [ ] Testability enabled
- [ ] Reliability guaranteed

### Documentation
- [ ] Architecture diagrams created
- [ ] Decision records maintained
- [ ] API documentation complete
- [ ] Deployment guides available
- [ ] Security practices documented

### Monitoring & Observability
- [ ] Logging strategy implemented
- [ ] Metrics collection configured
- [ ] Health checks enabled
- [ ] Alerting rules defined
- [ ] Performance monitoring active
