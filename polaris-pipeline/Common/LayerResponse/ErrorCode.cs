using System.Net;

namespace Common.LayerResponse;

public enum ErrorCode
{
    [ErrorCode(HttpStatusCode.FailedDependency,"") ]
    MdsError = 0
}

