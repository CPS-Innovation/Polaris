using Common.Domain.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Common.tests.Extensions;

public class JsonExtensionsTests
{
    [Fact]
    public void ToJson_IfNull_ReturnsEmptyString()
    {
        object? valueIn = null;
        var convertedItem = valueIn.ToJson();

        convertedItem.Should().BeEmpty();
    }

    [Fact]
    public void ToJson_ReturnsSerializedObject_AsExpected()
    {
        var objectToTest = new Sut(10, "Mark");
        var convertedItem = objectToTest.ToJson();
        var itemToTest = JsonConvert.SerializeObject(objectToTest);

        convertedItem.Should().Be(itemToTest);
    }
}

internal class Sut
{
    public int Id { get; }
    public string Name { get; }

    public Sut(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
