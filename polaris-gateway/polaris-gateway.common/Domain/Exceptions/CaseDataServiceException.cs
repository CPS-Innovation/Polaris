using System;

namespace PolarisGateway.Domain.Exceptions
{
    public class CaseDataServiceException : Exception
    {
        public CaseDataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}