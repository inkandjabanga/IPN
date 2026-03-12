using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using IPN.Api.Models;

namespace IPN.Api.Controllers;

/// <summary>
/// Controller for handling P2P (Person-to-Person) payment operations
/// </summary>
[ApiController]
[Route("api/p2p-payment")]
public class P2PPaymentController : ControllerBase
{
    // Using ConcurrentDictionary for thread-safe duplicate detection
    // In production, this should be replaced with a database or cache
    private static readonly ConcurrentDictionary<string, bool> ProcessedReferences = new();

    // Maximum allowed length for string inputs (security: prevents DoS via large payloads)
    private const int MaxReferenceLength = 50;
    private const int MaxAccountNumberLength = 20;

    /// <summary>
    /// Processes a P2P payment request
    /// Validates all inputs and returns appropriate response
    /// </summary>
    /// <param name="request">The payment request containing transaction details</param>
    /// <returns>Payment result with status, transaction ID, and message</returns>
    [HttpPost]
    public IActionResult ProcessPayment([FromBody] P2PPaymentRequest request)
    {
        // Security: Validate request is not null
        if (request == null)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: request body"
            });
        }

        // Validate ClientReference - Required field
        if (string.IsNullOrWhiteSpace(request.ClientReference))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: clientReference"
            });
        }

        // Security: Trim and validate length to prevent buffer overflow attacks
        request.ClientReference = request.ClientReference.Trim();
        if (request.ClientReference.Length > MaxReferenceLength)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "clientReference exceeds maximum length"
            });
        }

        // Validate SenderAccountNumber - Required field
        if (string.IsNullOrWhiteSpace(request.SenderAccountNumber))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: senderAccountNumber"
            });
        }

        // Security: Trim and validate sender account number length
        request.SenderAccountNumber = request.SenderAccountNumber.Trim();
        if (request.SenderAccountNumber.Length > MaxAccountNumberLength)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR002",
                TransactionId = null,
                Message = "Invalid account number format"
            });
        }

        // Validate ReceiverAccountNumber - Required field
        if (string.IsNullOrWhiteSpace(request.ReceiverAccountNumber))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: receiverAccountNumber"
            });
        }

        // Security: Trim and validate receiver account number length
        request.ReceiverAccountNumber = request.ReceiverAccountNumber.Trim();
        if (request.ReceiverAccountNumber.Length > MaxAccountNumberLength)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR002",
                TransactionId = null,
                Message = "Invalid account number format"
            });
        }

        // Validate Amount - Must be provided and valid
        if (request.Amount == null || !request.Amount.HasValue)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR004",
                TransactionId = null,
                Message = "Invalid amount"
            });
        }

        // Validate Currency - Required field
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: currency"
            });
        }

        // Security: Normalize currency to uppercase and trim
        request.Currency = request.Currency.Trim().ToUpperInvariant();

        // Validate Reference - Required field
        if (string.IsNullOrWhiteSpace(request.Reference))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Missing required field: reference"
            });
        }

        // Security: Trim and validate reference length
        request.Reference = request.Reference.Trim();
        if (request.Reference.Length > MaxReferenceLength)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "reference exceeds maximum length"
            });
        }

        // Validate Sender Account Number Format - Must be numeric only, minimum 10 digits
        if (!request.SenderAccountNumber.All(char.IsDigit) || request.SenderAccountNumber.Length < 10)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR002",
                TransactionId = null,
                Message = "Invalid account number format"
            });
        }

        // Validate Receiver Account Number Format - Must be numeric only, minimum 10 digits
        if (!request.ReceiverAccountNumber.All(char.IsDigit) || request.ReceiverAccountNumber.Length < 10)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR002",
                TransactionId = null,
                Message = "Invalid account number format"
            });
        }

        // Validate Currency - Must be NAD
        if (request.Currency != "NAD")
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR003",
                TransactionId = null,
                Message = "Invalid currency"
            });
        }

        // Validate Amount - Must be greater than 0
        if (request.Amount.Value <= 0)
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR004",
                TransactionId = null,
                Message = "Invalid amount"
            });
        }

        // Check for duplicate client reference - prevents duplicate transactions
        if (ProcessedReferences.ContainsKey(request.ClientReference))
        {
            return BadRequest(new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR001",
                TransactionId = null,
                Message = "Duplicate client reference"
            });
        }

        // Business Rule: Demo insufficient funds for amounts over 10000
        // In production, this would check actual account balance
        if (request.Amount.Value > 10000)
        {
            return StatusCode(402, new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR005",
                TransactionId = null,
                Message = "Insufficient funds"
            });
        }

        try
        {
            // Thread-safe addition to processed references
            ProcessedReferences.TryAdd(request.ClientReference, true);

            // Generate unique transaction ID with timestamp
            var transactionId = $"TXN{DateTime.Now:yyyyMMddHHmmss}";

            // Return successful payment response
            return Ok(new P2PPaymentResponse
            {
                Status = "SUCCESS",
                ErrorCode = null,
                TransactionId = transactionId,
                Message = "Payment processed successfully"
            });
        }
        catch (Exception ex)
        {
            // Log exception in production (use proper logging framework)
            // Security: Don't expose internal error details to client
            return StatusCode(500, new P2PPaymentResponse
            {
                Status = "FAILED",
                ErrorCode = "ERR006",
                TransactionId = null,
                Message = "Internal processing error"
            });
        }
    }
}
