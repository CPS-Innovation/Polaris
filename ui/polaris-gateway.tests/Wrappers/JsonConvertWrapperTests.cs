// using System;
// using AutoFixture;
// using FluentAssertions;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Newtonsoft.Json;
// using RumpoleGateway.Domain.CoreDataApi.ResponseTypes;
// using RumpoleGateway.Wrappers;
// using Xunit;

// namespace RumpoleGateway.tests.Wrappers
// {
//     public class JsonConvertWrapperTests
//     {
//         private readonly Fixture _fixture;
//         private readonly Mock<ILogger<JsonConvertWrapper>> _loggerMock;
//         private readonly Guid _correlationId;

//         public JsonConvertWrapperTests()
//         {
//             _fixture = new Fixture();
//             _loggerMock = new Mock<ILogger<JsonConvertWrapper>>();
//             _correlationId = _fixture.Create<Guid>();
//         }

//         [Fact]
//         public void SerializeObjectShouldDelegate()
//         {
//             var testObject = _fixture.Create<ResponseCaseDetails>();
//             var expectedSerializedRequest = JsonConvert.SerializeObject(testObject, Formatting.None, new JsonSerializerSettings());

//             var serializedObject = new JsonConvertWrapper(_loggerMock.Object).SerializeObject(testObject, _correlationId);

//             serializedObject.Should().BeEquivalentTo(expectedSerializedRequest);
//         }

//         [Fact]
//         public void DeserializeObjectShouldDelegate()
//         {
//             var testObject = _fixture.Create<ResponseCaseDetails>();
//             var serializedRequest = JsonConvert.SerializeObject(testObject, Formatting.None, new JsonSerializerSettings());

//             var deserializedRequest = new JsonConvertWrapper(_loggerMock.Object).DeserializeObject<ResponseCaseDetails>(serializedRequest, _correlationId);

//             testObject.Should().BeEquivalentTo(deserializedRequest);
//         }
//     }
// }
