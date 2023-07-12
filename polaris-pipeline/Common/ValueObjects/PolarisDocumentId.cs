using System.Collections.Generic;

namespace Common.ValueObjects
{
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects
    public class PolarisDocumentId : ValueObject
    {
        public string Value { get; private set; }

        public PolarisDocumentId() { }

        public PolarisDocumentId(string polarisDocumentIdValue)
        {
            Value = polarisDocumentIdValue;
        }

        public PolarisDocumentId(PolarisDocumentType polarisDocumentType, string value)
        {
            Value = polarisDocumentType switch
            {
                PolarisDocumentType.CmsDocument => $"CMS-{value}",
                PolarisDocumentType.PcdRequest => $"PCD-{value}",
                PolarisDocumentType.DefendantsAndCharges => $"DAC-{value}",
                _ => throw new System.NotImplementedException()
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return Value;
        }
        public override string ToString() => Value;

        public static implicit operator PolarisDocumentId(string value) => new PolarisDocumentId(value);

    }
}
