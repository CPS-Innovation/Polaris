using System;

namespace PolarisAuthHandover.Wrappers
{
    public interface IJsonConvertWrapper
    {
        string SerializeObject(object objectToSerialize, Guid correlationId);

        T DeserializeObject<T>(string value, Guid correlationId);
    }
}
