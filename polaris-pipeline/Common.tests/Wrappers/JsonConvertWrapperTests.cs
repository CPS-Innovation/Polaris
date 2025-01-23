using AutoFixture;
using Common.Wrappers;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Common.tests.Wrappers
{
    public class JsonConvertWrapperTests
    {
        private readonly Fixture _fixture;

        public JsonConvertWrapperTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void SerializeObjectShouldDelegate()
        {
            var testObject = _fixture.Create<StubRequest>();
            var expectedSerializedRequest = JsonSerializer.Serialize(testObject, new JsonSerializerOptions { WriteIndented = false });

            var serializedObject = new JsonConvertWrapper().SerializeObject(testObject);

            serializedObject.Should().BeEquivalentTo(expectedSerializedRequest);
        }

        [Fact]
        public void DeserializeObjectShouldDelegate()
        {
            var testObject = _fixture.Create<StubRequest>();
            var serializedRequest = JsonSerializer.Serialize(testObject, new JsonSerializerOptions { WriteIndented = false });

            var deserializedRequest = new JsonConvertWrapper().DeserializeObject<StubRequest>(serializedRequest);

            testObject.Should().BeEquivalentTo(deserializedRequest);
        }
    }
}
