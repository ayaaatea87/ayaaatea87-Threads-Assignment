using System;
namespace OrderProcessingSystem.Models;

public enum OrderStatus
{
    Pending,
    Validated,
    Rejected,
    PaymentFailed,
    Completed,
    Failed,
    Cancelled
}
