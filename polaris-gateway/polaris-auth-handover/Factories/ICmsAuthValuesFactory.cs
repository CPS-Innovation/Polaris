namespace PolarisAuthHandover.Factories
{
    public interface ICmsAuthValuesFactory
    {
        string SerializeCmsAuthValues(string cookies);

        string SerializeCmsAuthValues(string cookies, string cmsToken);
    }
}

