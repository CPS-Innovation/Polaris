namespace Common.Wrappers.Contracts
{
    public interface IJsonConvertWrapper
    {
        string SerializeObject(object objectToSerialize);

        T DeserializeObject<T>(string value);
    }
}
