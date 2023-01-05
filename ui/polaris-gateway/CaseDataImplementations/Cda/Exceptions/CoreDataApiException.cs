using System;
namespace RumpoleGateway.CaseDataImplementations.Cda.Exceptions
{
    public class CoreDataApiException : Exception
    {
        public CoreDataApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

