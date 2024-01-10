namespace polaris_common.Wrappers.Contracts
{
    public interface IJsonConvertWrapper
    {
        string SerializeObject(object objectToSerialize);

        T DeserializeObject<T>(string value);

        string SerializeObject(object objectToSerialize, Guid correlationId);

        T DeserializeObject<T>(string value, Guid correlationId);
    }
}
