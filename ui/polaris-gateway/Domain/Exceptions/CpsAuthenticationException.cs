using System;

namespace RumpoleGateway.Domain.Exceptions;

[Serializable]
public class CpsAuthenticationException: Exception
{
    public CpsAuthenticationException()
        : base("Invalid token. No authentication token was supplied.") { }
}
