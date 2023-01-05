using System;
namespace coordinator.Domain.Exceptions
{
    [Serializable]
    public class GeneratePdfHttpRequestFactoryException : Exception
    {
        public GeneratePdfHttpRequestFactoryException(string message) : base(message) { }
    }
}
