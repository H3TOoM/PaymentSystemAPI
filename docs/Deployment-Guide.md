# 🚀 Deployment Guide

## 📋 Prerequisites

### Development Environment
- .NET 10 SDK
- Visual Studio 2022 / VS Code
- SQL Server LocalDB or SQL Server Express

### Production Environment
- .NET 10 Hosting Bundle
- Windows Server / Linux Container
- SQL Server / Azure SQL Database
- Reverse Proxy (IIS / Nginx / Azure App Gateway)
- SSL Certificate

---

## 🏗️ Setup Steps

### 1. Clone Repository
```bash
git clone <repository-url>
cd "Payment System API"
```

### 2. Configuration

#### Development (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PaymentSystemDB_Dev;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "dev-secret-key-32-chars-minimum!",
    "Issuer": "PaymentSystemAPI-Dev",
    "Audience": "PaymentSystemClients-Dev",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Production (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${CONNECTION_STRING}"
  },
  "Jwt": {
    "SecretKey": "${JWT_SECRET}",
    "Issuer": "PaymentSystemAPI",
    "Audience": "PaymentSystemClients",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "ApplicationInsights": {
      "ConnectionString": "${APPLICATION_INSIGHTS_CONNECTION}"
    }
  }
}
```

### 3. Database Setup

#### Using EF Core Migrations
```bash
# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate --project PaymentSystem.Infrastructure

# Update database
dotnet ef database update --project PaymentSystem.Infrastructure
```

#### Manual Database Setup
```sql
-- Create database
CREATE DATABASE PaymentSystemDB;

-- Run migration scripts from PaymentSystem.Infrastructure/Migrations/
```

### 4. Build & Test
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests (if any)
dotnet test

# Run locally
dotnet run --project PaymentSystem.API --configuration Release
```

---

## 🐳 Docker Deployment

### Dockerfile
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

### Docker Compose
```yaml
version: '3.8'
services:
  paymentsystem-api:
    build: .
    ports:
      - "7001:8080"
      - "7002:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=PaymentSystemDB;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True
      - Jwt__SecretKey=YourProductionSecretKey32Chars!
      - Jwt__Issuer=PaymentSystemAPI
      - Jwt__Audience=PaymentSystemClients
    depends_on:
      - sqlserver
    networks:
      - paymentsystem-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - paymentsystem-network

volumes:
  sqlserver_data:

networks:
  paymentsystem-network:
    driver: bridge
```

### Docker Commands
```bash
# Build image
docker build -t paymentsystem-api .

# Run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f paymentsystem-api

# Stop
docker-compose down
```

---

## 🌐 Azure Deployment

### Azure App Service
```bash
# Create resource group
az group create --name PaymentSystemRG --location eastus

# Create App Service Plan
az appservice plan create --name PaymentSystemPlan --resource-group PaymentSystemRG --sku B1 --is-linux

# Create Web App
az webapp create --name PaymentSystemAPI --resource-group PaymentSystemRG --plan PaymentSystemPlan --runtime "DOTNETCORE|10.0"

# Configure Application Settings
az webapp config appsettings set --name PaymentSystemAPI --resource-group PaymentSystemRG --settings "ConnectionStrings__DefaultConnection=<your-connection-string>"
az webapp config appsettings set --name PaymentSystemAPI --resource-group PaymentSystemRG --settings "Jwt__SecretKey=<your-jwt-secret>"
az webapp config appsettings set --name PaymentSystemAPI --resource-group PaymentSystemRG --settings "Jwt__Issuer=PaymentSystemAPI"
az webapp config appsettings set --name PaymentSystemAPI --resource-group PaymentSystemRG --settings "Jwt__Audience=PaymentSystemClients"

# Deploy from ZIP
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
cd ..
az webapp deployment source config-zip --name PaymentSystemAPI --resource-group PaymentSystemRG --src deploy.zip
```

### Azure Container Instances
```bash
# Build and push to ACR
az acr build --registry paymentacr --image paymentsystem-api --file Dockerfile .

# Deploy to ACI
az container create \
  --resource-group PaymentSystemRG \
  --name paymentsystem-api \
  --image paymentacr.azurecr.io/paymentsystem-api:latest \
  --cpu 1 --memory 2 \
  --ports 8080 \
  --environment-variables ConnectionStrings__DefaultConnection=$CONNECTION_STRING Jwt__SecretKey=$JWT_SECRET
```

---

## 🐧 Linux Deployment

### Systemd Service
```ini
# /etc/systemd/system/paymentsystem-api.service
[Unit]
Description=Payment System API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/paymentsystem-api/PaymentSystem.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=paymentsystem-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection=<connection-string>
Environment=Jwt__SecretKey=<jwt-secret>

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable paymentsystem-api
sudo systemctl start paymentsystem-api
sudo systemctl status paymentsystem-api
```

### Nginx Reverse Proxy
```nginx
# /etc/nginx/sites-available/paymentsystem-api
server {
    listen 80;
    server_name api.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    ssl_certificate /etc/ssl/certs/yourdomain.com.crt;
    ssl_certificate_key /etc/ssl/private/yourdomain.com.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Correlation-ID $http_x_correlation_id;
    }
}
```

---

## 🪟 Windows Deployment

### IIS Configuration
```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\PaymentSystem.API.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ConnectionStrings__DefaultConnection" value="%CONNECTION_STRING%" />
          <environmentVariable name="Jwt__SecretKey" value="%JWT_SECRET%" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### PowerShell Deployment Script
```powershell
# Deploy-PaymentSystemAPI.ps1
param(
    [Parameter(Mandatory=$true)][string]$SiteName,
    [Parameter(Mandatory=$true)][string]$AppPool,
    [Parameter(Mandatory=$true)][string]$PhysicalPath,
    [Parameter(Mandatory=$true)][string]$ConnectionString,
    [Parameter(Mandatory=$true)][string]$JwtSecret
)

# Import WebAdministration module
Import-Module WebAdministration

# Create application pool
if (!(Test-Path "IIS:\AppPools\$AppPool")) {
    New-WebAppPool -Name $AppPool -Force
    Set-ItemProperty "IIS:\AppPools\$AppPool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
}

# Create website
if (!(Test-Path "IIS:\Sites\$SiteName")) {
    New-Website -Name $SiteName -Port 443 -PhysicalPath $PhysicalPath -ApplicationPool $AppPool
}

# Configure environment variables
Set-WebConfigurationProperty -Filter "/system.webServer/aspNetCore/environmentVariables" -Name "." -Value @{
    "ASPNETCORE_ENVIRONMENT" = "Production"
    "ConnectionStrings__DefaultConnection" = $ConnectionString
    "Jwt__SecretKey" = $JwtSecret
} -PSPath "IIS:\Sites\$SiteName"

# Restart app pool
Restart-WebAppPool -Name $AppPool

Write-Host "Deployment completed successfully!"
```

---

## 🔍 Monitoring & Health Checks

### Health Check Configuration
```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddCheck<ExternalServiceHealthCheck>("external-service");

// Health endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});
```

### Application Insights
```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);
builder.Services.AddApplicationInsightsTelemetryProcessor<CustomTelemetryProcessor>();
```

### Prometheus Metrics
```csharp
// In Program.cs
builder.Services.AddMetrics();
builder.Services.AddPrometheusMetrics();

// In pipeline
app.UsePrometheusMetrics();
app.UsePrometheusRequestLogging();
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions
```yaml
# .github/workflows/deploy.yml
name: Deploy Payment System API

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --configuration Release
      
    - name: Publish
      run: dotnet publish --configuration Release --output ./publish
      
    - name: Deploy to Azure
      if: github.ref == 'refs/heads/main'
      uses: azure/webapps-deploy@v2
      with:
        app-name: PaymentSystemAPI
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Azure DevOps
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  inputs:
    command: 'publish'
    publishWebProjects: true
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'

- task: AzureWebApp@1
  inputs:
    azureSubscription: 'Your Azure Subscription'
    appType: 'webAppLinux'
    appName: 'PaymentSystemAPI'
    package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
```

---

## 📝 Environment Variables

### Required Variables
| Variable | Description | Example |
|----------|-------------|---------|
| `CONNECTION_STRING` | SQL Server connection string | `Server=...;Database=...;...` |
| `JWT_SECRET` | JWT signing secret (32+ chars) | `YourSuperSecretKey32Chars!` |
| `JWT_ISSUER` | JWT token issuer | `PaymentSystemAPI` |
| `JWT_AUDIENCE` | JWT token audience | `PaymentSystemClients` |

### Optional Variables
| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment | `Production` |
| `JWT_EXPIRATION_MINUTES` | Token expiration | `60` |
| `APPLICATION_INSIGHTS_CONNECTION` | App Insights | - |
| `LOG_LEVEL` | Minimum log level | `Information` |

---

## 🔧 Troubleshooting

### Common Issues

#### Database Connection Failed
```bash
# Check connection string
dotnet ef database update --verbose

# Test SQL connection
sqlcmd -S your-server -d your-database -U your-user -P your-password
```

#### JWT Token Issues
```bash
# Verify secret key length (minimum 32 characters)
# Check token expiration
# Validate issuer/audience match
```

#### Rate Limiting
```bash
# Check rate limit headers
curl -I https://api.yourdomain.com/api/v1/health

# Adjust rate limits in Program.cs
```

#### SSL Certificate Issues
```bash
# Verify certificate chain
openssl s_client -connect api.yourdomain.com:443

# Check certificate expiration
openssl x509 -in cert.pem -noout -dates
```

### Performance Optimization
```csharp
// In Program.cs
builder.Services.AddResponseCompression();
builder.Services.AddOutputCache();

app.UseResponseCompression();
app.UseOutputCache();
```

---

## 📊 Scaling Considerations

### Horizontal Scaling
- Load balancer with sticky sessions (if needed)
- Distributed cache (Redis)
- Database read replicas
- Message queue for async operations

### Vertical Scaling
- Increase CPU/memory allocation
- Optimize database queries
- Use connection pooling
- Enable HTTP/2

---

## 🔄 Backup & Recovery

### Database Backup
```sql
-- SQL Server backup
BACKUP DATABASE PaymentSystemDB 
TO DISK = 'C:\Backups\PaymentSystemDB.bak'
WITH FORMAT, INIT;

-- Restore
RESTORE DATABASE PaymentSystemDB 
FROM DISK = 'C:\Backups\PaymentSystemDB.bak'
WITH REPLACE;
```

### Application Backup
```bash
# Backup published files
tar -czf paymentsystem-api-backup.tar.gz /opt/paymentsystem-api/

# Backup configuration
cp /etc/systemd/system/paymentsystem-api.service ./backup/
cp /etc/nginx/sites-available/paymentsystem-api ./backup/
```

---

## 📞 Support & Monitoring

### Log Locations
- **Application Logs**: `/var/log/paymentsystem-api/` (Linux) or Event Viewer (Windows)
- **Access Logs**: Nginx/IIS logs
- **Database Logs**: SQL Server logs

### Monitoring Tools
- **Application Insights**: Azure monitoring
- **Prometheus/Grafana**: Open-source monitoring
- **ELK Stack**: Log aggregation
- **Health Checks**: `/health` endpoint

### Alerting
- High error rate
- Database connection failures
- High memory/CPU usage
- SSL certificate expiration
