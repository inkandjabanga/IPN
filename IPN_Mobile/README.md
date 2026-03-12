# IPN Mobile - P2P Payment Application

## Overview

IPN Mobile is a Flutter-based mobile application for making Person-to-Person (P2P) payments. It integrates with the same API as the web application.

## Technology Stack

- **Framework**: Flutter
- **Language**: Dart
- **HTTP Client**: http package
- **Target Platforms**: Android, iOS, Web (Chrome)

## Project Structure

```
IPN_Mobile/
├── lib/
│   ├── main.dart              # App entry point
│   ├── screens/
│   │   ├── login_screen.dart  # Login screen
│   │   └── payment_screen.dart # Payment form screen
│   └── services/
│       └── api_service.dart    # API integration
├── pubspec.yaml               # Dependencies
└── README.md                 # This file
```

## Features

### Authentication
- Login screen with username/password
- Session-based navigation
- Logout functionality

### Payment Form
- Auto-generated client reference
- Sender account number validation (10+ digits, numeric only)
- Receiver account number validation (10+ digits, numeric only)
- Amount validation (> 0)
- Payment reference validation (max 50 chars, no leading spaces)
- Same account validation (sender and receiver cannot be the same)

### API Integration
- RESTful API communication
- JSON data exchange
- Error handling and user feedback

## Setup Instructions

### Prerequisites
- Flutter SDK 3.0+
- Dart SDK 3.0+

### 1. Install Dependencies
```bash
cd IPN_Mobile
flutter pub get
```

### 2. Run the Application

#### For Chrome (Web):
```bash
flutter run -d chrome
```

#### For Android Emulator:
```bash
flutter run -d emulator-5554
```

#### For iOS Simulator:
```bash
flutter run -d "iPhone 14"
```

### 3. Ensure API is Running
Make sure the API is running at:
- API URL: `http://localhost:5057/api/p2p-payment`
- Update to your localhost port number and keep the path the same
- update this variable value in the IPN_Mobile\lib\services folder
- static const String apiUrl = 'http://localhost:5057/api/p2p-payment';
  

## API Endpoint

### POST /api/p2p-payment

**Request:**
```json
{
  "clientReference": "REF-20260306-001",
  "senderAccountNumber": "1234567890",
  "receiverAccountNumber": "0987654321",
  "amount": 150.00,
  "currency": "NAD",
  "reference": "Lunch payment"
}
```

**Success Response:**
```json
{
  "status": "SUCCESS",
  "errorCode": null,
  "transactionId": "TXN202603060001",
  "message": "Payment processed successfully"
}
```

**Error Responses:**
| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| ERR001 | 400 | Missing required field / Duplicate |
| ERR002 | 400 | Invalid account number format |
| ERR003 | 400 | Invalid currency |
| ERR004 | 400 | Invalid amount |
| ERR005 | 402 | Insufficient funds |
| ERR006 | 500 | Internal processing error |

## Validation Rules

### Sender/Receiver Account
- Must be numeric only
- Minimum 10 digits
- Sender and receiver cannot be the same

### Amount
- Must be greater than 0
- Cannot be characters

### Reference
- Cannot be empty
- Cannot start with a space
- Maximum 50 characters

## Security

- Right-click disabled on web
- Input validation on all fields
- API errors handled gracefully
- Session management via navigation

## Running on Chrome

To run the application in Chrome:

1. Make sure Chrome is installed
2. Run: `flutter run -d chrome`
3. The app will open in Chrome at `http://localhost:port`

## Notes

- This mobile app uses the same API as the web application
- No dashboard included 
- Demo login accepts any credentials
- Data is not persisted (in-memory only)
