using System;

namespace PolarisGateway.Domain.Exceptions;

[Serializable]
public class CorrelationException : Exception
{
    public CorrelationException()
        : base("Invalid correlationId. A valid GUID is required.") { }
}