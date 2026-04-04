# 🔐 Security Best Practices & Configuration Guide

## 🛡️ Security Overview

This guide covers security best practices for the Payment System API, including authentication, authorization, data protection, and operational security measures.

---

## 🔑 Authentication & Authorization

### JWT Configuration

#### Secure JWT Settings
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationShouldBeAtLeast32Characters!",
    "Issuer": "PaymentSystemAPI",
    "Audience": "PaymentSystemClients",
    "ExpirationMinutes": 60
  }
}
```

#### Security Requirements
- **Secret Key**: Minimum 32 characters, high entropy
- **Algorithm**: Always use HMAC-SHA256 (default)
- **Expiration**: Short-lived tokens (15-60 minutes)
- **Issuer/Audience**: Validate both to prevent token reuse

#### Production JWT Configuration
```csharp
// In Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(1), // Reduced clock skew
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});
```

### Password Security

#### Password Hashing
```csharp
// Uses ASP.NET Core PasswordHasher V3
// Automatically includes:
// - PBKDF2 with HMAC-SHA256
// - 10,000 iterations (configurable)
// - 128-bit salt
// - Secure random salt per password

public class PasswordHasher<T> : IPasswordHasher<T>
{
    public string HashPassword(T user, string password)
    {
        // Automatic salt generation
        // Configurable iteration count
        // Built-in best practices
    }
}
```

#### Password Policy (FluentValidation)
```csharp
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")
                .WithMessage("Password must contain at least one special character.")
            .NotEqual(x => x.Email).WithMessage("Password cannot be the same as email.")
            .NotContain("password", StringComparison.OrdinalIgnoreCase).WithMessage("Password cannot contain the word 'password'.");
    }
}
```

---

## 🛡️ API Security

### Rate Limiting

#### Token Bucket Configuration
```csharp
// In RateLimitingMiddleware.cs
private static RateLimiter CreateRateLimiter()
{
    return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 100,        // Maximum tokens
        QueueLimit = 10,         // Queue for excess requests
        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
        TokensPerPeriod = 20,    // Refill rate
        AutoReplenishment = true
    });
}
```

#### Rate Limiting Headers
```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1640995200
X-RateLimit-Retry-After: 30
```

### Input Validation

#### Request Validation Pipeline
```csharp
// 1. Model Binding Validation
[ApiController]
public class BaseController : ControllerBase
{
    [ProducesResponseType(400)]
    public IActionResult ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed.",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
            });
        }
        return null;
    }
}

// 2. FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

// 3. Global Exception Handling
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
```

#### SQL Injection Prevention
```csharp
// Using EF Core parameterized queries (automatic)
// Never concatenate SQL strings
public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
{
    // Safe: Parameterized query
    return await _dbContext.Users
        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    
    // Unsafe: Never do this!
    // return await _dbContext.Users
    //     .FromSqlRaw($"SELECT * FROM Users WHERE Email = '{email}'")
    //     .FirstOrDefaultAsync();
}
```

---

## 🔒 Data Protection

### Sensitive Data Handling

#### Never Log Sensitive Information
```csharp
// ❌ BAD: Logging passwords or tokens
_logger.LogInformation("User login attempt: {Email}, {Password}", email, password);

// ✅ GOOD: Log only non-sensitive data
_logger.LogInformation("User login attempt: {Email}, CorrelationId: {CorrelationId}", 
    email, correlationId);

// ❌ BAD: Logging full request body with sensitive data
_logger.LogInformation("Request body: {RequestBody}", requestBody);

// ✅ GOOD: Sanitize before logging
var sanitizedRequest = SanitizeForLogging(requestBody);
_logger.LogInformation("Request body: {RequestBody}", sanitizedRequest);
```

#### Data Sanitization
```csharp
public static class DataSanitizer
{
    private static readonly string[] SensitiveFields = 
    { "password", "token", "secret", "key", "creditcard", "ssn" };

    public static string SanitizeForLogging(string json)
    {
        foreach (var field in SensitiveFields)
        {
            var pattern = $"\"{field}\"\\s*:\\s*\"[^\"]*\"";
            json = Regex.Replace(json, pattern, $"\"{field}\":\"[REDACTED]\"", RegexOptions.IgnoreCase);
        }
        return json;
    }
}
```

### Encryption at Rest

#### Database Encryption
```sql
-- Enable Transparent Data Encryption (TDE)
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'StrongMasterKeyPassword';
CREATE CERTIFICATE PaymentSystemCert WITH SUBJECT = 'PaymentSystem Database';
CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE PaymentSystemCert;

ALTER DATABASE PaymentSystemDB SET ENCRYPTION ON;
```

#### Connection String Security
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=PaymentSystemDB;User Id=app_user;Password={{VAULT:DB_PASSWORD}};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

---

## 🌐 Network Security

### HTTPS Configuration

#### TLS Configuration
```csharp
// In Program.cs
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});

// HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

#### Security Headers
```csharp
// Custom middleware for security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    
    await next();
});
```

### CORS Configuration
```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://app.yourdomain.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID")
              .AllowCredentials();
    });
});

app.UseCors("ProductionPolicy");
```

---

## 🔍 Auditing & Logging

### Audit Trail Implementation
```csharp
// Domain entity for auditing
public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; }
    public string TargetType { get; private set; }
    public string Details { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
}

// Automatic audit logging
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = GetCurrentUserId(),
            Action = request.GetType().Name,
            TargetType = typeof(TResponse).Name,
            Details = JsonSerializer.Serialize(request),
            CreatedAtUtc = DateTime.UtcNow,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        try
        {
            var response = await next();
            auditLog.Details += $" | Success: {JsonSerializer.Serialize(response)}";
            await _auditRepository.AddAsync(auditLog, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            auditLog.Details += $" | Error: {ex.Message}";
            await _auditRepository.AddAsync(auditLog, cancellationToken);
            throw;
        }
    }
}
```

### Security Event Logging
```csharp
// Security events to monitor
public enum SecurityEventType
{
    LoginSuccess,
    LoginFailure,
    PasswordChange,
    AccountLockout,
    SuspiciousActivity,
    RateLimitExceeded,
    InvalidToken,
    UnauthorizedAccess
}

// Logging security events
public class SecurityLogger
{
    public void LogSecurityEvent(SecurityEventType eventType, string userId, string details, string ipAddress)
    {
        _logger.LogWarning("Security Event: {EventType}, User: {UserId}, IP: {IpAddress}, Details: {Details}",
            eventType, userId, ipAddress, details);
            
        // Send to SIEM system
        _siemClient.SendSecurityEvent(new SecurityEvent
        {
            EventType = eventType,
            UserId = userId,
            IpAddress = ipAddress,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

---

## 🚨 Incident Response

### Security Incident Response Plan

#### 1. Detection
```csharp
// Anomaly detection
public class AnomalyDetectionMiddleware
{
    private readonly Dictionary<string, DateTime> _failedAttempts = new();
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        // Check for multiple failed logins
        if (IsSuspiciousActivity(clientIp))
        {
            _securityLogger.LogSecurityEvent(
                SecurityEventType.SuspiciousActivity, 
                null, 
                "Multiple failed attempts detected", 
                clientIp);
                
            // Block IP temporarily
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }
        
        await _next(context);
    }
}
```

#### 2. Immediate Response
```bash
# Block malicious IP (Linux)
iptables -A INPUT -s MALICIOUS_IP -j DROP

# Block malicious IP (Windows PowerShell)
New-NetFirewallRule -DisplayName "Block Malicious IP" -Direction Inbound -RemoteAddress MALICIOUS_IP -Action Block

# Rotate JWT secret
# Update appsettings.json with new secret
# Restart application
```

#### 3. Investigation
```sql
-- Query audit logs for suspicious activity
SELECT * FROM AuditLogs 
WHERE CreatedAtUtc >= DATEADD(hour, -24, GETUTCDATE())
  AND (Action LIKE '%Login%' OR Action LIKE '%Transfer%')
  AND Details LIKE '%ERROR%'
ORDER BY CreatedAtUtc DESC;

-- Check for unusual transaction patterns
SELECT UserId, COUNT(*) as TransactionCount, SUM(Amount) as TotalAmount
FROM Transactions 
WHERE CreatedAtUtc >= DATEADD(hour, -24, GETUTCDATE())
GROUP BY UserId
HAVING COUNT(*) > 50 OR SUM(Amount) > 10000;
```

---

## 🔧 Configuration Security

### Environment Variables
```bash
# Production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=PaymentSystemDB;..."
export Jwt__SecretKey="YourProductionSecretKey32Characters!"
export Jwt__Issuer="PaymentSystemAPI"
export Jwt__Audience="PaymentSystemClients"
```

### Azure Key Vault Integration
```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration["KeyVault:Uri"]),
        new DefaultAzureCredential());
}

// Access secrets
var jwtSecret = builder.Configuration["Jwt:SecretKey"]; // Retrieved from Key Vault
```

### Secret Management
```csharp
// Development: User Secrets
dotnet user-secrets set "Jwt:SecretKey" "YourDevelopmentSecretKey"

// Production: Azure Key Vault / AWS Secrets Manager
// Never store secrets in appsettings.json for production
```

---

## 🧪 Security Testing

### Security Headers Test
```bash
# Test security headers
curl -I https://api.yourdomain.com/api/v1/health

# Expected headers:
# X-Content-Type-Options: nosniff
# X-Frame-Options: DENY
# X-XSS-Protection: 1; mode=block
# Strict-Transport-Security: max-age=31536000; includeSubDomains
```

### Penetration Testing Checklist
- [ ] SQL injection testing
- [ ] XSS testing
- [ ] CSRF protection
- [ ] Authentication bypass attempts
- [ ] Authorization testing
- [ ] Rate limiting effectiveness
- [ ] Input validation testing
- [ ] Error handling information disclosure

### Automated Security Scanning
```bash
# OWASP ZAP
docker run -t owasp/zap2docker-stable zap-baseline.py -t https://api.yourdomain.com

# Security Code Analysis
dotnet tool install --global security-scan
security-scan --project PaymentSystem.API
```

---

## 📊 Monitoring & Alerting

### Security Metrics
```csharp
// Custom metrics for security
public class SecurityMetrics
{
    private readonly Counter<int> _loginAttempts;
    private readonly Counter<int> _failedLogins;
    private readonly Counter<int> _rateLimitHits;
    
    public void RecordLoginAttempt() => _loginAttempts.Add(1);
    public void RecordFailedLogin() => _failedLogins.Add(1);
    public void RecordRateLimitHit() => _rateLimitHits.Add(1);
}
```

### Alerting Rules
```yaml
# Prometheus alerting rules
groups:
- name: security
  rules:
  - alert: HighFailedLoginRate
    expr: rate(failed_logins_total[5m]) > 10
    for: 2m
    labels:
      severity: warning
    annotations:
      summary: "High rate of failed login attempts detected"
      
  - alert: RateLimitExceeded
    expr: rate(rate_limit_hits_total[5m]) > 5
    for: 1m
    labels:
      severity: critical
    annotations:
      summary: "Rate limiting frequently exceeded"
```

---

## 🔄 Regular Security Tasks

### Daily
- [ ] Review security logs for anomalies
- [ ] Monitor failed login attempts
- [ ] Check for unusual transaction patterns

### Weekly
- [ ] Review and rotate secrets if needed
- [ ] Update security patches
- [ ] Review access logs
- [ ] Test backup and recovery procedures

### Monthly
- [ ] Security audit and penetration testing
- [ ] Review and update security policies
- [ ] Update dependencies for security patches
- [ ] Review user access and permissions

### Quarterly
- [ ] Full security assessment
- [ ] Update incident response plan
- [ ] Security training for team
- [ ] Review and update architecture for security

---

## 📞 Security Contacts

### Security Team
- **Security Lead**: security@yourcompany.com
- **Incident Response**: incident@yourcompany.com
- **Security Hotline**: +1-555-SECURITY

### External Resources
- **CERT/CC**: cert@cert.org
- **Security Vendor**: security-vendor@yourcompany.com

---

## 📚 Security References

- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [SANS Top 25](https://www.sans.org/top25-software-errors/)

---

## 🚨 Emergency Procedures

### Immediate Actions (0-1 hour)
1. **Isolate affected systems**
2. **Preserve evidence**
3. **Activate incident response team**
4. **Assess scope and impact**
5. **Communicate with stakeholders**

### Short-term Actions (1-24 hours)
1. **Contain the breach**
2. **Identify root cause**
3. **Implement temporary fixes**
4. **Monitor for continued activity**
5. **Document all actions**

### Long-term Actions (1-7 days)
1. **Implement permanent fixes**
2. **Review and update security policies**
3. **Conduct post-incident review**
4. **Update monitoring and alerting**
5. **Communicate resolution**

---

## 📋 Security Configuration Checklist

### Pre-Deployment Checklist
- [ ] JWT secret is 32+ characters and stored securely
- [ ] Database connection uses encryption
- [ ] HTTPS is enforced with valid certificates
- [ ] Security headers are configured
- [ ] CORS policy is restrictive
- [ ] Rate limiting is enabled
- [ ] Input validation is implemented
- [ ] Error messages don't expose sensitive information
- [ ] Logging includes correlation IDs
- [ ] Audit trail is enabled
- [ ] Security monitoring is configured
- [ ] Backup procedures are tested
- [ ] Incident response plan is updated

### Post-Deployment Checklist
- [ ] Security headers are present in responses
- [ ] Rate limiting is working correctly
- [ ] Authentication and authorization are working
- [ ] Input validation is preventing attacks
- [ ] Logging and monitoring are functioning
- [ ] Performance impact is acceptable
- [ ] Documentation is updated
- [ ] Team is trained on new features
