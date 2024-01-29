using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    public static class ListExtensions
    {
        public static bool ValuesAreEqual(this IList<long> values, int maxCount)
        {
            if (!values.Any()) return false;

            var firstValue = values.First();

            return values.Count() == maxCount && !values.Any(x => x != firstValue);
        }
    }
}