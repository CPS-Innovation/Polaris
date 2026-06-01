// <copyright file="AuthenticationResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace HkuiFunctionsIntegrationTests.Dto;

public record AuthenticationResponse(
    string Cookies,
    string Token,
    DateTimeOffset ExpiryTime);
