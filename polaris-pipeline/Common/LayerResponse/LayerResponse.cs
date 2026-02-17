using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Common.LayerResponse;

public class LayerResponse<T> : ILayerResponse<T>
{
    private readonly List<Error> _errorList = new();

    public LayerResponse()
    {
        
    }

    public LayerResponse(T content)
    {
        Content = content;
    }

    public T Content { get; set; }

    public IEnumerable<Error> Errors => _errorList.AsReadOnly();

    public bool HasError => _errorList.Any();

    // Find way to use Telemetry events here rather than ILogger
    public LayerResponse<T> AddError(ErrorCode errorCode, 
       // ILogger logger, 
        string message = "", [CallerMemberName] string callerMemberName = "")
    {
        var error = new Error(errorCode, message, callerMemberName); 
        _errorList.Add(error);
        // logger.LogError(error.ToString());
        return this;
    }

    public LayerResponse<T> AddError(Error error)
    {
        _errorList.Add(error);
        return this;
    }

    public LayerResponse<T> AddErrors<TK>(ILayerResponse<TK> response)
    {
        if (response is not null)
        {
            _errorList.AddRange(response.Errors);
        }
        return this;
    }
}
