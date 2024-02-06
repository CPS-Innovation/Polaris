using Common.Extensions;
using Xunit;

namespace Common.tests.Extensions
{
    public class ListExtensionsTests
    {
        private const int MaxCount = 5;

        [Fact]
        public void ValuesAreEqual_ReturnsFalse_WhenListIsEmpty()
        {
            var items = new List<long>();

            Assert.False(items.ValuesAreEqual(MaxCount));
        }

        [Fact]
        public void ValuesAreEqual_ReturnsFalse_WhenValuesAreNotEqual_AndMaxCountIsNotReached()
        {
            var item = new List<long> { 1, 2, 3 };

            Assert.False(item.ValuesAreEqual(MaxCount));
        }

        [Fact]
        public void ValuesAreEqual_ReturnsFalse_WhenValuesAreNotEqual_AndMaxCountIsReached()
        {
            var item = new List<long> { 1, 2, 3, 4, 5 };

            Assert.False(item.ValuesAreEqual(MaxCount));
        }

        [Fact]
        public void ValuesAreEqual_ReturnsFalse_WhenValuesAreEqual_AndMaxCountIsNotReached()
        {
            var item = new List<long> { 5, 5, 5, 5 };

            Assert.False(item.ValuesAreEqual(MaxCount));
        }

        [Fact]
        public void ValuesAreEqual_ReturnsTrue_WhenValuesAreEqual_AndMaxCountIsReached()
        {
            var item = new List<long> { 5, 5, 5, 5, 5 };

            Assert.True(item.ValuesAreEqual(MaxCount));
        }
    }
}