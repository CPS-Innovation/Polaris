using System;
using System.Net;

namespace Common.Exceptions;
//
// Summary:
//     Not found exception class.
public class NotFoundException : Exception
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} ({key}) was not found.")
    {
    }
}
