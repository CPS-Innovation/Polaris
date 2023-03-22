using System;
namespace coordinator.Services.DocumentToggle.Exceptions
{
    public class DocumentToggleException : Exception
    {
        public DocumentToggleException(string message, Exception innerException = null)
        : base(message, innerException)
        {
        }
    }
}