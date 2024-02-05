using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    public static class ExceptionExtension
    {
        public static string NestedMessage(this Exception exception)
        {
            if (exception.InnerException == null)
            {
                return exception.Message;
            }

            var innerExceptionMessage = NestedMessage(exception.InnerException);

            return $"{exception.Message}; {innerExceptionMessage}";
        }
        
        public static string ToFormattedString(this Exception exception)
        {
            var messages = exception
                .GetAllExceptions()
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(e => e.Message.Trim());
            var flattened = string.Join("; ", messages); // <-- the separator here
            return flattened;
        }
    
        private static IEnumerable<Exception> GetAllExceptions(this Exception exception)
        {
            yield return exception;

            if (exception is AggregateException aggrEx)
            {
                foreach (var innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
                {
                    yield return innerEx;
                }
            }
            else if (exception.InnerException != null)
            {
                foreach (var innerEx in exception.InnerException.GetAllExceptions())
                {
                    yield return innerEx;
                }
            }
        }
    }
}
