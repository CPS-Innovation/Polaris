namespace PolarisGateway.Exceptions;

[Serializable]
public class CmsAuthenticationException : Exception
{
    public CmsAuthenticationException()
        : base("Invalid cms auth values. A string is expected.") { }
}