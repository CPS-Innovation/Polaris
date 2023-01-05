using System;
namespace coordinator.Domain.Exceptions
{
    [Serializable]
    public class TextExtractorHttpRequestFactoryException : Exception
    {
        public TextExtractorHttpRequestFactoryException(string message) : base(message) { }
    }
}
