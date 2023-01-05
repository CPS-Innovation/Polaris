using Azure.Identity;
using coordinator.Factories;
using FluentAssertions;
using Xunit;

namespace coordinator.tests.Factories
{
    public class DefaultAzureCredentialFactoryTests
    {
        [Fact]
        public void GetCredential_ReturnsDefaultAzureCredential()
        {
            var factory = new DefaultAzureCredentialFactory();

            var credentials = factory.Create();

            credentials.Should().BeOfType<DefaultAzureCredential>();
        }
    }
}

