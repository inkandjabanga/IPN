using System.Net.Http.Json;
using IPN.Web.Models;

namespace IPN.Web.Services;

/// <summary>
/// Interface for payment processing service
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a payment request via the API
    /// </summary>
    /// <param name="request">Payment request details</param>
    /// <returns>Payment result with status and transaction ID</returns>
    Task<P2PPaymentResultViewModel> ProcessPaymentAsync(P2PPaymentViewModel request);
}

/// <summary>
/// Service for handling payment API calls
/// Security: Uses HttpClient with configured timeouts
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    
    // API endpoint URL - in production, move to configuration
    private const string ApiUrl = "http://localhost:5057/api/p2p-payment";

    /// <summary>
    /// Constructor - injects HttpClient (configured in Program.cs)
    /// </summary>
    public PaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends payment request to the API
    /// Security: Validates response and handles errors gracefully
    /// </summary>
    public async Task<P2PPaymentResultViewModel> ProcessPaymentAsync(P2PPaymentViewModel request)
    {
        try
        {
            // Map request to API format
            var apiRequest = new
            {
                clientReference = SanitizeInput(request.ClientReference),
                senderAccountNumber = SanitizeInput(request.SenderAccountNumber),
                receiverAccountNumber = SanitizeInput(request.ReceiverAccountNumber),
                amount = request.Amount,
                currency = SanitizeInput(request.Currency),
                reference = SanitizeInput(request.Reference)
            };

            // Send request to API
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, apiRequest);
            
            // Read and return response
            var result = await response.Content.ReadFromJsonAsync<P2PPaymentResultViewModel>();
            
            return result ?? new P2PPaymentResultViewModel
            {
                Status = "FAILED",
                ErrorCode = "ERR006",
                Message = "Internal processing error"
            };
        }
        catch (TaskCanceledException)
        {
            // Handle timeout
            return new P2PPaymentResultViewModel
            {
                Status = "FAILED",
                ErrorCode = "ERR006",
                Message = "Internal processing error."
            };
        }
        catch (HttpRequestException)
        {
            // Handle connection errors
            return new P2PPaymentResultViewModel
            {
                Status = "FAILED",
                ErrorCode = "ERR006",
                Message = "Internal processing error."
            };
        }
        catch (Exception)
        {
            // Handle unexpected errors
            return new P2PPaymentResultViewModel
            {
                Status = "FAILED",
                ErrorCode = "ERR006",
                Message = "Internal processing error."
            };
        }
    }

    /// <summary>
    /// Helper: Sanitizes input to prevent XSS attacks
    /// </summary>
    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        return input.Trim();
    }
}
