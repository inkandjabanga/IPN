import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../services/api_service.dart';
import 'login_screen.dart';

/// Payment Screen
/// Main screen for making P2P payments
/// Contains form validation and API integration
class PaymentScreen extends StatefulWidget {
  final String username;

  const PaymentScreen({super.key, required this.username});

  @override
  State<PaymentScreen> createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  final _formKey = GlobalKey<FormState>();
  final _apiService = ApiService();
  
  // Form controllers
  late TextEditingController _clientRefController;
  late TextEditingController _senderAccountController;
  late TextEditingController _receiverAccountController;
  late TextEditingController _amountController;
  late TextEditingController _referenceController;
  
  bool _isLoading = false;
  Map<String, dynamic>? _paymentResult;

  @override
  void initState() {
    super.initState();
    // Generate unique client reference
    _clientRefController = TextEditingController(
      text: 'REF-${DateTime.now().toString().replaceAll('-', '').substring(0, 8)}-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}',
    );
    _senderAccountController = TextEditingController();
    _receiverAccountController = TextEditingController();
    _amountController = TextEditingController();
    _referenceController = TextEditingController();
  }

  @override
  void dispose() {
    _clientRefController.dispose();
    _senderAccountController.dispose();
    _receiverAccountController.dispose();
    _amountController.dispose();
    _referenceController.dispose();
    super.dispose();
  }

  /// Validate sender account number
  String? validateSenderAccount(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please enter sender account number';
    }
    if (value.contains(RegExp(r'[a-zA-Z]'))) {
      return 'No letters are allowed';
    }
    if (value.length < 10) {
      return 'Minimum 10 digits required';
    }
    return null;
  }

  /// Validate receiver account number
  String? validateReceiverAccount(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please enter receiver account number';
    }
    if (value.contains(RegExp(r'[a-zA-Z]'))) {
      return 'No letters are allowed';
    }
    if (value.length < 10) {
      return 'Minimum 10 digits required';
    }
    return null;
  }

  /// Validate amount
  String? validateAmount(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please enter amount';
    }
    final amount = double.tryParse(value);
    if (amount == null) {
      return 'Please enter a valid number';
    }
    if (amount <= 0) {
      return 'Amount must be greater than 0';
    }
    return null;
  }

  /// Validate reference
  String? validateReference(String? value) {
    if (value == null || value.isEmpty) {
      return 'Please enter payment reference';
    }
    if (value.startsWith(' ')) {
      return 'Reference cannot start with a space';
    }
    if (value.length > 50) {
      return 'Maximum 50 characters allowed';
    }
    return null;
  }

  /// Check if sender and receiver are the same
  bool get _accountsAreSame {
    return _senderAccountController.text.isNotEmpty &&
        _receiverAccountController.text.isNotEmpty &&
        _senderAccountController.text == _receiverAccountController.text;
  }

  /// Submit payment to API
  Future<void> _submitPayment() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    if (_accountsAreSame) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Sender and receiver account numbers cannot be the same'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    setState(() {
      _isLoading = true;
      _paymentResult = null;
    });

    try {
      final result = await _apiService.processPayment(
        clientReference: _clientRefController.text.trim(),
        senderAccountNumber: _senderAccountController.text.trim(),
        receiverAccountNumber: _receiverAccountController.text.trim(),
        amount: double.parse(_amountController.text),
        currency: 'NAD',
        reference: _referenceController.text.trim(),
      );

      setState(() {
        _paymentResult = result;
      });

      // Show result dialog
      if (!mounted) return;
      _showResultDialog(result);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Error: $e'),
          backgroundColor: Colors.red,
        ),
      );
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  /// Show payment result dialog
  void _showResultDialog(Map<String, dynamic> result) {
    final isSuccess = result['status'] == 'SUCCESS';
    
    showDialog(
      context: context,
      barrierDismissible: false,
      barrierColor: Colors.black54,
      builder: (dialogContext) => AlertDialog(
        title: Row(
          children: [
            Icon(
              isSuccess ? Icons.check_circle : Icons.error,
              color: isSuccess ? Colors.green : Colors.red,
            ),
            const SizedBox(width: 8),
            Expanded(child: Text(isSuccess ? 'Payment Successful' : 'Payment Failed')),
          ],
        ),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildResultRow('Status', result['status'] ?? ''),
              if (result['transactionId'] != null)
                _buildResultRow('Transaction ID', result['transactionId']),
              if (result['errorCode'] != null)
                _buildResultRow('Error Code', result['errorCode']),
              _buildResultRow('Message', result['message'] ?? ''),
            ],
          ),
        ),
        actions: [
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {
                Navigator.of(dialogContext).pop(true);
              },
              child: const Text('OK'),
            ),
          ),
        ],
        actionsAlignment: MainAxisAlignment.center,
        actionsPadding: const EdgeInsets.all(16),
      ),
    ).then((_) {
      _resetForm();
    });
  }

  Widget _buildResultRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(
              '$label:',
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }

  /// Generate a new client reference
  void _generateNewReference() {
    setState(() {
      _clientRefController.text = 
          'REF-${DateTime.now().toString().replaceAll('-', '').substring(0, 8)}-${DateTime.now().millisecondsSinceEpoch.toString().substring(7)}';
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('New reference generated'),
        duration: Duration(seconds: 1),
      ),
    );
  }

  /// Reset form for new payment
  void _resetForm() {
    _generateNewReference();
    _senderAccountController.clear();
    _receiverAccountController.clear();
    _amountController.clear();
    _referenceController.clear();
    _formKey.currentState?.reset();
    setState(() {
      _paymentResult = null;
    });
  }

  /// Logout and go back to login
  void _logout() {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (context) => const LoginScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.credit_card, size: 24),
            SizedBox(width: 8),
            Text('P2P Payment'),
          ],
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(
              child: Text(
                'Welcome, ${widget.username}',
                style: const TextStyle(fontSize: 14),
              ),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: _logout,
            tooltip: 'Logout',
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Client Reference
              TextFormField(
                controller: _clientRefController,
                decoration: InputDecoration(
                  labelText: 'Client Reference',
                  border: const OutlineInputBorder(),
                  suffixIcon: IconButton(
                    icon: const Icon(Icons.refresh),
                    onPressed: _generateNewReference,
                    tooltip: 'Generate new reference',
                  ),
                ),
              ),
              const SizedBox(height: 16),

              // Currency (Read-only)
              TextFormField(
                initialValue: 'NAD',
                readOnly: true,
                decoration: InputDecoration(
                  labelText: 'Currency',
                  border: const OutlineInputBorder(),
                  filled: true,
                  fillColor: Colors.grey[100],
                ),
              ),
              const SizedBox(height: 16),

              // Sender Account Number
              TextFormField(
                controller: _senderAccountController,
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                decoration: const InputDecoration(
                  labelText: 'Sender Account Number',
                  hintText: 'Enter sender account (10+ digits)',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.person_outline),
                ),
                validator: validateSenderAccount,
                onChanged: (_) => setState(() {}),
              ),
              const SizedBox(height: 16),

              // Receiver Account Number
              TextFormField(
                controller: _receiverAccountController,
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                decoration: InputDecoration(
                  labelText: 'Receiver Account Number',
                  hintText: 'Enter receiver account (10+ digits)',
                  border: const OutlineInputBorder(),
                  prefixIcon: const Icon(Icons.person),
                  errorText: _accountsAreSame 
                      ? 'Sender and receiver cannot be the same' 
                      : null,
                ),
                validator: validateReceiverAccount,
                onChanged: (_) => setState(() {}),
              ),
              const SizedBox(height: 16),

              // Amount
              TextFormField(
                controller: _amountController,
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d{0,2}')),
                ],
                decoration: const InputDecoration(
                  labelText: 'Amount (NAD)',
                  hintText: 'Enter amount',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.attach_money),
                ),
                validator: validateAmount,
              ),
              const SizedBox(height: 16),

              // Payment Reference
              TextFormField(
                controller: _referenceController,
                maxLength: 50,
                decoration: const InputDecoration(
                  labelText: 'Payment Reference',
                  hintText: 'Enter payment reference (max 50 chars)',
                  border: OutlineInputBorder(),
                  prefixIcon: Icon(Icons.description),
                  counterText: '',
                ),
                validator: validateReference,
              ),
              const SizedBox(height: 24),

              // Submit Button
              ElevatedButton.icon(
                onPressed: _isLoading ? null : _submitPayment,
                icon: _isLoading
                    ? const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(Icons.send),
                label: Text(_isLoading ? 'Processing...' : 'Submit Payment'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF1565C0),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
              const SizedBox(height: 16),

              // Reset Button
              OutlinedButton.icon(
                onPressed: _isLoading ? null : _resetForm,
                icon: const Icon(Icons.refresh),
                label: const Text('Reset'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: const Color(0xFF1565C0),
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  side: const BorderSide(color: Color(0xFF1565C0)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
