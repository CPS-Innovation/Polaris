
using Common.Services.PiiService.AllowedWords;
using FluentAssertions;
using Moq;
using Xunit;

namespace coordinator.tests.Services.PiiServiceTests.AllowedWords
{
    public class PiiAllowedListServiceTests
    {
        private readonly Mock<IPiiAllowedList> _piiAllowedList;
        private readonly PiiAllowedListService _piiAllowedListService;

        public PiiAllowedListServiceTests()
        {
            _piiAllowedList = new Mock<IPiiAllowedList>();
            _piiAllowedListService = new PiiAllowedListService(_piiAllowedList.Object);
        }

        [Fact]
        public void WhenCheckingPiiAllowedList_AndTheValueDoesNotExist_FalseIsReturned()
        {
            _piiAllowedList.Setup(x => x.GetWords()).Returns([]);

            var result = _piiAllowedListService.Contains("witness", "PersonType");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenCheckingPiiAllowedList_AndTheValueDoesExist_TrueIsReturned()
        {
            _piiAllowedList.Setup(x => x.GetWords()).Returns(
            [
                new("witness", "PersonType")
            ]);

            var result = _piiAllowedListService.Contains("witness", "PersonType");

            result.Should().BeTrue();
        }
    }
}