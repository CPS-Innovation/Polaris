using System;

namespace Common.Exceptions;

public class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(string message)
        : base(message)
    {
    }
}