using Common.Constants;
using System;

namespace Common.Exceptions;

[Serializable]
public class CmsAuthValuesMissingException : Exception
{
    public CmsAuthValuesMissingException()
        : base($"Invalid Cms Auth Values. A \"{HttpHeaderKeys.CmsAuthValues}\" header or cookie is expected")
    {
    }
}