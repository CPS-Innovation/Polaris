﻿using System;

namespace RumpoleGateway.Domain.Exceptions;

[Serializable]
public class CpsAuthorizationException : Exception
{
    public CpsAuthorizationException()
        : base("Token validation failed") {  }
}
