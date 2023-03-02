using System;

namespace PolarisGateway.Wrappers
{
    public interface IJsonConvertWrapper
    {
        string SerializeObject(object objectToSerialize, Guid correlationId);

        T DeserializeObject<T>(string value, Guid correlationId);
    }
}
