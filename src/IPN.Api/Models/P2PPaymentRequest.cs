using System.ComponentModel.DataAnnotations;

namespace IPN.Api.Models;

public class P2PPaymentRequest
{
    [Required]
    public string? ClientReference { get; set; }
    
    [Required]
    public string? SenderAccountNumber { get; set; }
    
    [Required]
    public string? ReceiverAccountNumber { get; set; }
    
    [Required]
    public decimal? Amount { get; set; }
    
    [Required]
    public string Currency { get; set; } = "NAD";
    
    [Required]
    public string? Reference { get; set; }
}
