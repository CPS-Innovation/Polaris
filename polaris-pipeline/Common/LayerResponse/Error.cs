using System.Net;

namespace Common.LayerResponse;

public class Error
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public string? Message { get; set; }
    public string? Description { get; set; }
    public string? CallerMemberName { get; set; }

    public Error()
    {
            
    }

    public Error(ErrorCode errorCode, string? message, string? callerMemberName)
    {
        HttpStatusCode = errorCode.GetHttpStatusCode();
        Message = message;
        Description = errorCode.GetDescription();
        CallerMemberName = callerMemberName;
    }

    public Error(HttpStatusCode statusCode, string? message, string? callerMemberName)
    {
        HttpStatusCode = statusCode;
        Message = message;
        CallerMemberName = callerMemberName;
    }

    public override string ToString()
    {
        return $"HttpStatusCode: {HttpStatusCode}, {HttpStatusCode} {Message} from {CallerMemberName}";
    }
}
