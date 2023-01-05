using System;
using System.Diagnostics.CodeAnalysis;

namespace Common.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ClientTokenException : Exception
    {
        public ClientTokenException(string message) : base(message) { }
    }
}