using System;
namespace OrderProcessingSystem.Exceptions;

public sealed class DatabaseWriteException : Exception
{
    public DatabaseWriteException(string message)
        : base(message)
    {
    }
}
