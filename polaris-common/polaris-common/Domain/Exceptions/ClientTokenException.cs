using System.Diagnostics.CodeAnalysis;

namespace polaris_common.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ClientTokenException : Exception
    {
        public ClientTokenException(string message) : base(message) { }
    }
}