namespace PolarisGateway.Validators
{
    public class ValidatableRequest<T>
    {
        public T Value { get; set; }

        public bool IsValid { get; set; }

        public string RequestJson { get; set; }
    }
}
