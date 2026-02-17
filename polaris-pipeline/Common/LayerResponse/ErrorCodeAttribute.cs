using System.ComponentModel;
using System.Net;

namespace Common.LayerResponse;

class ErrorCodeAttribute : DescriptionAttribute
{
    private HttpStatusCode HttpStatusCodeValue {  get; }
    public virtual HttpStatusCode HttpStatusCode => HttpStatusCodeValue;

    public ErrorCodeAttribute(HttpStatusCode httpStatusCode, string description) : base(description)
    {
        HttpStatusCodeValue = httpStatusCode;
    }
}
