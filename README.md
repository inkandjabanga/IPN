# Instant Payments Namibia (IPN) - P2P Payment Application

## Overview

This is a Person-to-Person (P2P) payment application built with .NET 9, featuring a RESTful API and a responsive web interface using HTML5, Bootstrap 5, and jQuery. The application demonstrates secure payment processing with comprehensive validation and security measures.

## Project Structure

```
IPN/
├── src/
│   ├── IPN.Api/                    # RESTful API project
│   │   ├── Controllers/             # API controllers with security
│   │   └── Models/                 # Request/Response models
│   └── IPN.Web/                    # MVC Web application
│       ├── Controllers/              # MVC controllers with auth
│       ├── Models/                 # View models
│       ├── Services/                # Business services
│       └── Views/                  # Razor views
├── IPN.sln                         # Solution file
└── README.md                       # This file
```

## Technology Stack

### Backend
- **Framework**: .NET 9, ASP.NET Core
- **Language**: C# 12
- **API**: RESTful API with JSON

### Frontend
- **Framework**: Bootstrap 5
- **Language**: HTML5, JavaScript (jQuery)
- **UI**: Responsive design with mobile-first approach

### Security
- **Session Management**: In-memory session with secure cookies
- **Input Validation**: Server-side and client-side validation
- **Security Headers**: X-Frame-Options, XSS Protection, HSTS

## Application Navigation

### 1. Login Page (`/Home/Login`)
- Entry point of the application
- Requires username and password
- Demo credentials: `BON` / `BON`

### 2. Payment Page (`/Home/Index`)
- Main payment form (requires login)
- Dashboard with payment statistics
- Payment history table(in-memory)
- Split view: Payment form (right), Dashboard (left) on desktop
- On mobile: Payment form appears first

### 3. Logout (`/Home/Logout`)
- Clears session and redirects to login

## Setup Instructions

### Prerequisites
- .NET 9 SDK or later
- SQL Server Express (optional - for database connection)
- A modern web browser

### 1. Clone or Download the Repository
```bash
git clone <repository-url>
cd IPN
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Update Connection String (Optional)
The connection string is configured in both `appsettings.json` files:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=.\\SQLEXPRESS01;Initial Catalog=IPN;TrustServerCertificate=True;Integrated Security=True"
}
```

### 4. Build the Solution
```bash
dotnet build
```

### 5. Run the Application

**Terminal 1 - Start the API:**
```bash
cd src/IPN.Api
dotnet run
```
- API: `http://localhost:5057`
- Swagger Documentation: `http://localhost:5057/swagger`

**Terminal 2 - Start the Web App:**
```bash
cd src/IPN.Web
dotnet run
```
- Web Application: `http://localhost:5174`

### 6. Access the Application
1. Open browser to `http://localhost:5174`
2. Login with demo credentials (BON/BON)
3. Make payments and view dashboard

## API Documentation

### Endpoint
**POST** `/api/p2p-payment`

### Request Fields
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| clientReference | string | Yes | Unique transaction reference |
| senderAccountNumber | string | Yes | Sender account number (10+ digits) |
| receiverAccountNumber | string | Yes | Receiver account number (10+ digits) |
| amount | number | Yes | Payment amount (> 0) |
| currency | string | Yes | Must equal "NAD" |
| reference | string | Yes | Payment reference (1-50 chars) |

### Success Response (200 OK)
```json
{
  "status": "SUCCESS",
  "errorCode": null,
  "transactionId": "TXN202603060001",
  "message": "Payment processed successfully"
}
```

### Error Responses
| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| ERR001 | 400 | Missing required field / Duplicate reference |
| ERR002 | 400 | Invalid account number format |
| ERR003 | 400 | Invalid currency (must be NAD) |
| ERR004 | 400 | Invalid amount |
| ERR005 | 402 | Insufficient funds |
| ERR006 | 500 | Internal processing error |

## How the Integration Was Implemented

### API to Web Integration
- **API Endpoint**: The RESTful API runs on `http://localhost:5057/api/p2p-payment`
- **HTTP Client**: Web app uses `HttpClient` via `IHttpClientFactory` to make API calls
- **Service Layer**: `PaymentService` class handles all API communication
- **JSON Communication**: Data is sent and received as JSON format

### Web Consuming API
- **jQuery AJAX**: The web app uses jQuery's `$.ajax()` to POST payment requests to the API
- **Asynchronous Calls**: API calls are made asynchronously without page reload
- **Response Handling**: Success and error responses are handled and displayed to users
- **Real-time Updates**: Dashboard updates immediately after payment response

### Integration Flow
1. User fills payment form on web app
2. Client-side validation validates inputs
3. Confirmation modal shows payment summary
4. On confirm, jQuery AJAX sends POST request to API
5. API validates and processes payment
6. API returns JSON response (SUCCESS/FAILED)
7. Web app displays result and updates dashboard

### Code Implementation
- **API Controller**: `P2PPaymentController.cs` - handles POST requests
- **Payment Service**: `PaymentService.cs` - wraps HttpClient for API calls
- **View**: `Index.cshtml` - uses jQuery AJAX to call API endpoint

### Security Considerations
- **Input Validation**: Both web and API validate inputs independently
- **Error Handling**: API errors are caught and displayed gracefully
- **Timeout**: HTTP client configured with 30-second timeout
- **No CORS Issues**: API and Web run on different ports locally

## Security Implementation

### 1. Authentication
- **Session-based authentication**: All payment operations require login
- **Secure session cookies**: HttpOnly, SameSite=Lax, secure in production
- **Session timeout**: 30 minutes of inactivity
- **Why**: For security reasons, payments must be done from a secured authenticated application

### 2. Input Validation
- **Server-side validation**: All inputs validated in API controller
- **Client-side validation**: Real-time validation with jQuery (Client side validation for fast application performance - no constant requests from and to server.) - On-page validation
- **Input sanitization**: Trim and encode user inputs
- **Length limits**: Maximum length validation to prevent buffer overflow
- **Numeric-only**: Account numbers validated as digits only
- **Same account numbers validation**: Validating of the account numbers, user can not make payment to the same account


### 3. Security Headers
The application implements the following security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000`
- `Referrer-Policy: strict-origin-when-cross-origin`

### 4. API Security
- **Input validation**: All fields validated before processing
- **Duplicate prevention**: ConcurrentDictionary for thread-safe duplicate detection
- **Error handling**: No internal error details exposed to clients
- **Numeric validation**: Account numbers must be digits only
- **Length constraints**: Maximum lengths enforced on all string inputs
- **Amount**: An amount can not be a Character

### 5. Web Application Security
- **Right-click disabled**: Prevents context menu access for application security reasons
- **No caching**: Sensitive pages not cached
- **Preloader**: Prevents visual exposure during loading
- **Sanitized output**: HTML encoding on user-provided data

### 6. Transport Security
- **HTTPS redirection**: HTTP requests redirected to HTTPS
- **Secure cookies**: Session cookies marked as secure



## Assumptions Made

1. **Application Login**: For security reasons, payments must be done from a secured authenticated application
2. **API Authentication**: In production, the APIs must have authentication (OAuth2/JWT) to prevent attacks
3. **Right Click Disabled**: For application security reasons, context menu is disabled
4. **Dashboard**: A mini dashboard for the user to view payment history and statistics
5. **No Database**: Demo uses in-memory storage; production requires database
6. **Currency**: Only NAD (Namibian Dollar) is accepted
7. **Insufficient Funds**: Amounts over 10,000 NAD trigger insufficient funds for demo purposes

### 8. Authentication on API
- **Current State**: Demo authentication on web app only
- **Production Recommendation**: API should implement OAuth2/JWT authentication to prevent unauthorized API attacks



9. **Confirmation Modal** - User needs to make sure he/she is sending the correct amount to the correct account
- Payment summary before submission
- Amount, sender, receiver, reference display
- Confirm/Cancel options

10. **Dashboard (Mini)** - User needs to see his/her payment history
- Total payments count
- Successful payments count
- Failed payments count
- Total amount processed (NAD)

11. **Payment History**
- Reference, Receiver, Amount, Status columns
- Scrollable table for multiple payments
- Badge indicators (Green for SUCCESS, Red for FAILED)


## Web Application Features
**Payment Form**
- Client reference auto-generation (GUID-based)
- Sender/Receiver account validation (10+ digits, numeric only)
- Amount validation (> 0)
- Payment reference (max 50 characters, no leading spaces)
- Real-time validation with green tick for valid fields
- Make Payment Submit button enabled only when form is completely valid

- **Login Screen**: Simulated authentication
- **Single Page Application**: Payment form and results on one page
- **jQuery AJAX**: Asynchronous API calls without page reload
- **Modern UI**: Bootstrap 5 with custom styling
- **Client-side Validation**: Real-time form validation
- **Responsive Design**: Works on desktop and mobile (payment form first on mobile)
- **Error Handling**: All API error codes handled and displayed

## License

This is a demonstration project for the IPN Developer Integration Challenge.
