import 'dart:convert';
import 'package:http/http.dart' as http;

/// API Service for P2P Payments
/// Handles all HTTP communication with the backend API
class ApiService {
  // API endpoint URL
  static const String apiUrl = 'http://localhost:5057/api/p2p-payment';
  
  /// Processes a P2P payment by calling the API
  /// Returns a Map with the API response
  Future<Map<String, dynamic>> processPayment({
    required String clientReference,
    required String senderAccountNumber,
    required String receiverAccountNumber,
    required double amount,
    required String currency,
    required String reference,
  }) async {
    try {
      final response = await http.post(
        Uri.parse(apiUrl),
        headers: {
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'clientReference': clientReference,
          'senderAccountNumber': senderAccountNumber,
          'receiverAccountNumber': receiverAccountNumber,
          'amount': amount,
          'currency': currency,
          'reference': reference,
        }),
      );

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      } else {
        // Try to parse error response
        try {
          return jsonDecode(response.body);
        } catch (e) {
          return {
            'status': 'FAILED',
            'errorCode': 'ERR006',
            'transactionId': null,
            'message': 'An unexpected error occurred',
          };
        }
      }
    } catch (e) {
      return {
        'status': 'FAILED',
        'errorCode': 'ERR006',
        'transactionId': null,
        'message': 'Unable to connect to the API. Please ensure the API is running.',
      };
    }
  }
}
