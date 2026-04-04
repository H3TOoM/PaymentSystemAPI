# 📚 API Documentation Guide

## 🌐 Using Scalar UI (Development)

When running the API in development mode:

1. Navigate to: `https://localhost:7001/` (or your configured port)
2. You'll see the **Scalar API Documentation** interface
3. Click **"Authorize"** button to set your JWT token
4. Enter `Bearer <your-jwt-token>` in the modal
5. All authenticated endpoints will now work

### Scalar Features
- **Modern UI** with dark/light theme
- **Try endpoints** directly from browser
- **Auto-generated cURL** commands
- **JWT Bearer** authentication support
- **Export to** OpenAPI/Postman

---

## 📋 Endpoint Details

### 🔐 Authentication

#### Register User
- **URL**: `POST /api/v1/auth/register`
- **Body**:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "P@ssw0rd123!"
}
```
- **Response** (201):
```json
{
  "success": true,
  "message": "User registered successfully.",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "correlationId": "..."
}
```

#### Login
- **URL**: `POST /api/v1/auth/login`
- **Body**:
```json
{
  "email": "john@example.com",
  "password": "P@ssw0rd123!"
}
```
- **Response** (200):
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "john@example.com",
    "name": "John Doe"
  },
  "correlationId": "..."
}
```

### 💰 Wallets (Authenticated)

#### Get Wallet
- **URL**: `GET /api/v1/wallets/{userId}`
- **Headers**: `Authorization: Bearer <token>`
- **Response** (200):
```json
{
  "success": true,
  "message": "Wallet retrieved successfully.",
  "data": {
    "id": "b5e8f4a2-9c4d-4e2f-8a7b-3d9c1e6f2a8b",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "currency": "USD",
    "balance": 150.75,
    "createdAtUtc": "2026-04-04T12:34:56Z"
  },
  "correlationId": "..."
}
```

#### Deposit
- **URL**: `POST /api/v1/wallets/deposit`
- **Headers**: `Authorization: Bearer <token>`
- **Body**:
```json
{
  "amount": 100.00
}
```
- **Response** (200):
```json
{
  "success": true,
  "message": "Deposit successful.",
  "data": null,
  "correlationId": "..."
}
```

#### Withdraw
- **URL**: `POST /api/v1/wallets/withdraw`
- **Headers**: `Authorization: Bearer <token>`
- **Body**:
```json
{
  "amount": 50.00
}
```
- **Response** (200):
```json
{
  "success": true,
  "message": "Withdrawal successful.",
  "data": null,
  "correlationId": "..."
}
```

### 🔄 Transactions (Authenticated)

#### Transfer Money
- **URL**: `POST /api/v1/transactions/transfer`
- **Headers**: `Authorization: Bearer <token>`
- **Body**:
```json
{
  "receiverUserId": "a7b9c1d3-5e6f-4a8b-9c2d-3e4f5a6b7c8d",
  "amount": 25.00,
  "referenceId": "TRX-12345"
}
```
- **Response** (200):
```json
{
  "success": true,
  "message": "Transfer initiated successfully.",
  "data": {
    "transactionId": "c8e7d6b5-4a3c-2e1f-9b8a-7d6c5b4a3c2e",
    "status": "Completed"
  },
  "correlationId": "..."
}
```

#### Get Transaction History
- **URL**: `GET /api/v1/transactions/{userId}`
- **Headers**: `Authorization: Bearer <token>`
- **Response** (200):
```json
{
  "success": true,
  "message": "Transaction history retrieved successfully.",
  "data": [
    {
      "id": "c8e7d6b5-4a3c-2e1f-9b8a-7d6c5b4a3c2e",
      "walletId": "b5e8f4a2-9c4d-4e2f-8a7b-3d9c1e6f2a8b",
      "referenceId": "TRX-12345",
      "type": "Transfer",
      "amount": 25.00,
      "createdAtUtc": "2026-04-04T13:45:30Z",
      "status": "Completed"
    }
  ],
  "correlationId": "..."
}
```

### 🏥 Health Check

#### Service Health
- **URL**: `GET /health`
- **Response** (200):
```json
{
  "status": "Healthy",
  "timestamp": "2026-04-04T14:20:00Z",
  "version": "1.0.0"
}
```

---

## 🚨 Error Responses

### Validation Errors (400)
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "Email is required.",
    "Password must be at least 8 characters long."
  ],
  "correlationId": "..."
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "message": "Unauthorized.",
  "errors": [],
  "correlationId": "..."
}
```

### Not Found (404)
```json
{
  "success": false,
  "message": "Resource not found.",
  "errors": [],
  "correlationId": "..."
}
```

### Server Error (500)
```json
{
  "success": false,
  "message": "An internal server error occurred.",
  "errors": [],
  "correlationId": "..."
}
```

---

## 🔧 Postman Collection

1. Import `docs/PostmanCollection.json` into Postman
2. Update `baseUrl` variable to your API endpoint
3. Execute "Register" then "Login" to get token
4. Token is automatically stored for authenticated requests
5. All requests include correlation ID headers

### Collection Features
- **Auto-generated correlation IDs**
- **Automatic token extraction** from login response
- **Request/response tests** for validation
- **Environment variables** for easy configuration

---

## 📝 Testing Tips

### Using cURL
```bash
# Register
curl -X POST "https://localhost:7001/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","password":"P@ssw0rd123!"}'

# Login
TOKEN=$(curl -s -X POST "https://localhost:7001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"P@ssw0rd123!"}' | \
  jq -r '.data.token')

# Get Wallet
curl -X GET "https://localhost:7001/api/v1/wallets/YOUR_USER_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Correlation-ID: $(uuidgen)"
```

### Using JavaScript/Fetch
```javascript
// Login
const loginResponse = await fetch('/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'john@example.com',
    password: 'P@ssw0rd123!'
  })
});
const loginData = await loginResponse.json();
const token = loginData.data.token;

// Get Wallet
const walletResponse = await fetch(`/api/v1/wallets/${userId}`, {
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Correlation-ID': crypto.randomUUID()
  }
});
const walletData = await walletResponse.json();
```

---

## 🔄 API Versioning

The API supports versioning via:
- **URL Path**: `/api/v1/...` (default)
- **Header**: `X-Version: 1.0`
- **Query**: `?version=1.0`

Current version: **v1.0**

---

## 📊 Rate Limiting

- **Token Bucket**: 100 tokens initial capacity
- **Refill Rate**: 20 tokens per 10 seconds
- **Queue**: 10 requests max when rate exceeded
- **Response**: `429 Too Many Requests` when limit exceeded

Headers included in responses:
- `X-RateLimit-Remaining`: Tokens left
- `X-RateLimit-Retry-After`: Seconds to wait (when limited)

---

## 🔍 Correlation IDs

Every request/response includes:
- **Request Header**: `X-Correlation-ID` (optional, client-provided)
- **Response Header**: `X-Correlation-ID` (always present)
- **Response Body**: `correlationId` field

Use this ID to trace requests across:
- Application logs
- Database operations
- External service calls

---

## 📋 OpenAPI Specification

The complete OpenAPI 3.0 specification is available at:
- **Development**: `https://localhost:7001/openapi/v1.json`
- **Production**: `https://your-domain.com/api/v1/openapi.json`

Use this to generate client SDKs or import into other API tools.
