using System;

namespace PolarisGateway.Domain.Exceptions;

[Serializable]
public class UpstreamAuthenticationException : Exception
{
    public UpstreamAuthenticationException()
        : base("Invalid upstream token. A string is expected.") { }
}
