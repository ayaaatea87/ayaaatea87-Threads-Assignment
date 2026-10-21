using System;
namespace OrderProcessingSystem.Models;


public class PaymentTransaction
{
    public int OrderId { get; set; }
    public int Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
