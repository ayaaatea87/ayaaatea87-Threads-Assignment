using System;

namespace OrderProcessingSystem.Exceptions;

public sealed class SmtpUnavailableException : Exception
{
    public SmtpUnavailableException(string message)
        : base(message)
    {
    }
}
