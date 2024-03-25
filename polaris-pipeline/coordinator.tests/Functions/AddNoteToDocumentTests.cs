using AutoFixture;
using Common.Dto.Request;
using Common.Wrappers;
using coordinator.Functions;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace coordinator.tests.Functions
{
    public class AddNoteToDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<ILogger<AddNoteToDocument>> _mockLogger;
        private readonly Mock<IDdeiClient> _mockDdeiClient;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IValidator<AddDocumentNoteDto>> _mockRequestValidator;
        private readonly AddNoteToDocument _addNoteToDocument;

        public AddNoteToDocumentTests()
        {
            var cmsAuthValues = _fixture.Create<string>();
        }
    }
}