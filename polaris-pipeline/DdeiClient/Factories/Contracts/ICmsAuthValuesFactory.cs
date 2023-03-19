namespace Ddei.Factories.Contracts
{
    public interface ICmsAuthValuesFactory
    {
        string SerializeCmsAuthValues(string cookies);

        string SerializeCmsAuthValues(string cookies, string cmsToken);
    }
}

