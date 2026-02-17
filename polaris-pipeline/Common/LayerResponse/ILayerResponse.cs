using System.Collections.Generic;

namespace Common.LayerResponse;

public interface ILayerResponse<T>
{
    T Content { get; }
    IEnumerable<Error> Errors { get; }
    bool HasError { get; }
}

