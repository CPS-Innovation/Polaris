using System;
namespace Common.Services.DocumentToggle.Domain
{
    public class DocumentToggleException : Exception
    {
        public DocumentToggleException(string message, Exception innerException = null)
        : base(message, innerException)
        {
        }
    }
}