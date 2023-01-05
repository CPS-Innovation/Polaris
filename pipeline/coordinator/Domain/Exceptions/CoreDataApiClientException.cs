using System;
namespace coordinator.Domain.Exceptions
{
    [Serializable]
    public class CoreDataApiClientException : Exception
    {
        public CoreDataApiClientException(string message): base(message)
        {
        }
    }
}
