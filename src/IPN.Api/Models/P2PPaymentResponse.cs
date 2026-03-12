namespace IPN.Api.Models;

public class P2PPaymentResponse
{
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? TransactionId { get; set; }
    public string Message { get; set; } = string.Empty;
}
