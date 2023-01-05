using AutoFixture;
using Common.Domain.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.Results;
using Xunit;

namespace Common.tests.Extensions;

public class ValidationResultExtensionsTests
{
    private readonly Fixture _fixture;

    public ValidationResultExtensionsTests()
    {
        _fixture = new Fixture();
    }
    
    [Fact]
    public void FlattenErrors_NoErrors_ShouldStillBeFlattened_AsExpected()
    {
        var sut = _fixture.Create<ValidationResult>();
        sut.Errors = new List<ValidationFailure>();

        var test = sut.FlattenErrors();

        test.Should().BeEmpty();
    }
    
    [Fact]
    public void FlattenErrors_WithErrors_ShouldBeFlattened_AsExpected()
    {
        var sut = _fixture.Create<ValidationResult>();
        sut.Errors = _fixture.CreateMany<ValidationFailure>(2).ToList();

        var test = sut.FlattenErrors();

        using (new AssertionScope())
        {
            test.Should().Contain(sut.Errors[0].PropertyName);
            test.Should().Contain(sut.Errors[0].ErrorMessage);
            test.Should().Contain(sut.Errors[1].PropertyName);
            test.Should().Contain(sut.Errors[1].ErrorMessage);
        }
    }
}
