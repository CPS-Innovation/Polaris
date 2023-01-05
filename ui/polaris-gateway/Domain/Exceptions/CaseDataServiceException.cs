using System;

namespace RumpoleGateway.Domain.Exceptions
{
    public class CaseDataServiceException : Exception
    {
        public CaseDataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}