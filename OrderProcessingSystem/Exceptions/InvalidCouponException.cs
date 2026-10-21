using System;
namespace OrderProcessingSystem.Exceptions;

public sealed class InvalidCouponException : Exception
{
    public InvalidCouponException(string message)
        : base(message)
    {
    }
}
