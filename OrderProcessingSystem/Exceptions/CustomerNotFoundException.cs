using System;
namespace OrderProcessingSystem.Exceptions;

public sealed class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException(string message)
        : base(message)
    {
    }
}
