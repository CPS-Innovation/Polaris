using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Adapters;
using Common.Domain.Requests;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessTokenTests
    {
        private readonly string _onBehalfOfAccessToken;
        private readonly Guid _correlationId;
        private readonly string _accessToken;
        private readonly string _requestedScopes;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly GetOnBehalfOfAccessToken _getOnBehalfOfAccessToken;

        public GetOnBehalfOfAccessTokenTests()
        {
            var fixture = new Fixture();
            
            _accessToken = fixture.Create<string>();
            _onBehalfOfAccessToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();
            _requestedScopes = fixture.Create<string>();

            var identityClientAdapterMock = new Mock<IIdentityClientAdapter>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();
            
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest(_accessToken, _requestedScopes, _correlationId));

            identityClientAdapterMock.Setup(client => client.GetAccessTokenOnBehalfOfAsync(_accessToken, It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(_onBehalfOfAccessToken);

            var mockLogger = new Mock<ILogger<GetOnBehalfOfAccessToken>>();
            _getOnBehalfOfAccessToken = new GetOnBehalfOfAccessToken(identityClientAdapterMock.Object, mockLogger.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Run_ThrowsWhenAccessToken_InPayload_IsNullOrEmpty(string accessToken)
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest(accessToken, _requestedScopes, _correlationId));

            await Assert.ThrowsAsync<ArgumentException>(() => _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_ThrowsWhenCorrelationId_InPayload_IsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest(_accessToken, _requestedScopes, Guid.Empty));

            await Assert.ThrowsAsync<ArgumentException>(() => _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Run_ThrowsWhenRequestedScopes_InPayload_AreNullOrEmpty(string requestedScopes)
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetOnBehalfOfTokenRequest>())
                .Returns(new GetOnBehalfOfTokenRequest(_accessToken, requestedScopes, _correlationId));

            await Assert.ThrowsAsync<ArgumentException>(() => _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsAccessToken()
        {
            var caseDetails = await _getOnBehalfOfAccessToken.Run(_mockDurableActivityContext.Object);

            caseDetails.Should().Be(_onBehalfOfAccessToken);
        }
    }
}
