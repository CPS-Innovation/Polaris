using System;

namespace PolarisAuthHandover.Domain.Exceptions
{
    public class DocumentServiceException : Exception
    {
        public DocumentServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}