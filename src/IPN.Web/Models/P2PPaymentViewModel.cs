namespace IPN.Web.Models;

public class P2PPaymentViewModel
{
    public string ClientReference { get; set; } = string.Empty;
    public string SenderAccountNumber { get; set; } = string.Empty;
    public string ReceiverAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NAD";
    public string Reference { get; set; } = string.Empty;
}

public class P2PPaymentResultViewModel
{
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? TransactionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess => Status == "SUCCESS";
}
