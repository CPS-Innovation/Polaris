using Common.Constants;
using Ddei.Domain.CaseData.Args.Core;
using System.Net;

namespace DdeiClient.Factories;

public abstract class BaseDdeiClientRequestFactory
{
    protected const string CorrelationId = "Correlation-Id";
    protected virtual void AddAuthHeaders(HttpRequestMessage request, MdsBaseArgDto arg) // VERIFY if MdsBaseArgDto is correct here
    {
        request.Headers.Add(HttpHeaderKeys.CmsAuthValues, arg.CmsAuthValues);
        request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
    }

    protected virtual string Encode(string param) => WebUtility.UrlEncode(param);
}