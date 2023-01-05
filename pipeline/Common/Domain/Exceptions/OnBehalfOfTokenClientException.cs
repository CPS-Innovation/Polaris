using System;
using System.Diagnostics.CodeAnalysis;

namespace Common.Domain.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class OnBehalfOfTokenClientException : Exception
    {
        public OnBehalfOfTokenClientException(string message) : base(message) { }
    }
}
