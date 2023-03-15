using System;

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
    }
}
