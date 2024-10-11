using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Case;
using FluentValidation.TestHelper;
using PolarisGateway.Validators;
using Xunit;

namespace PolarisGateway.Tests.Validators
{
    public class ReorderStatementsValidatorTests
    {
        private readonly Fixture _fixture;

        public ReorderStatementsValidatorTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task ReorderingStatements_WhenEmpty_ReturnsValidationError()
        {
            var orderedStatements = _fixture.Create<OrderedStatementsDto>();
            orderedStatements.Statements = null;

            var reorderStatementValidator = new ReorderStatementsValidator();
            var validationResult = await reorderStatementValidator.TestValidateAsync(orderedStatements);

            validationResult.ShouldHaveValidationErrorFor(x => x.Statements);
        }

        [Fact]
        public void ReorderingStatements_CorrectChildValidator_Loaded()
        {
            var orderedStatements = _fixture.Create<OrderedStatementsDto>();
            orderedStatements.Statements = new List<OrderedStatementDto>();

            var reorderStatementValidator = new ReorderStatementsValidator();
            reorderStatementValidator.ShouldHaveChildValidator(x => x.Statements, typeof(OrderedStatementValidator));
        }
    }
}