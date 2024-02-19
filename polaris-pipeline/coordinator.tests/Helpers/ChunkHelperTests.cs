using Xunit;
using AutoFixture;
using coordinator.Helpers.ChunkHelper;
using FluentAssertions;
using System.Linq;

namespace coordinator.tests.Helpers
{
    public class ChunkHelperTests
    {
        private const int MaxCharacterLimit = 8192;
        private const int DocumentIdLength = 11;

        private readonly Fixture _fixture;

        public ChunkHelperTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineData(true)] // Exceeds limit
        [InlineData(false)] // Stays under limit
        public void ChunkStringListByMaxCharacterCount_WhenStringListIsLongerThanMaxCharacter_ReturnsChunkedStrings(bool exceedsLimit)
        {
            // Arrange
            int charactersPerDocumentId = DocumentIdLength + 4; // Double quotes and comma
            int charactersPerChunk = MaxCharacterLimit - 4; // Brackets and brackets delimiter
            int maxDocumentIdsPerChunk = charactersPerChunk / charactersPerDocumentId;

            _fixture.Register(() => new string(_fixture.CreateMany<char>(DocumentIdLength).ToArray()));

            var stringList = exceedsLimit ?
                _fixture.CreateMany<string>(maxDocumentIdsPerChunk + 2).ToList() :
                _fixture.CreateMany<string>(maxDocumentIdsPerChunk - 1).ToList();

            // Act
            var result = ChunkHelper.ChunkStringListByMaxCharacterCount(stringList, MaxCharacterLimit);

            // Assert
            if (exceedsLimit)
            {
                result.Should().HaveCountGreaterThan(1);
            }
            else
            {
                result.Should().HaveCount(1);
            }
        }
    }
}
