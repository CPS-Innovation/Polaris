// <copyright file="IRedactionLoggerClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using coordinator.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public interface IRedactionLoggerClient
{
    public Task<Stream> CreateRedactionLog(CreateRedactionLogsRequest redactionLogRequest, Guid correlationId);
}
