using System;
namespace coordinator.Domain.Exceptions
{
    public class DocumentToggleException : Exception
    {
        public DocumentToggleException(string message, Exception innerException = null)
        : base(message, innerException)
        {
        }
    }
}