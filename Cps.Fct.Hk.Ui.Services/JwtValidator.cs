// <copyright file="JwtValidator.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Provides validation logic for a JWT token.
/// </summary>
public class JwtValidator(ILogger<JwtValidator> logger, IConfiguration config, IJwksFetcher jwksFetcher)
    : IJwtValidator
{
    private readonly ILogger<JwtValidator> logger = logger;
    private readonly IConfiguration config = config;
    private readonly IJwksFetcher jwksFetcher = jwksFetcher;

    /// <inheritdoc />
    public async Task<bool> ValidateJwtSignatureAsync(string idToken)
    {
        try
        {
            // Get the tenant and application IDs
            string msalTenantId = this.GetConfigValue("MsalTenantId");
            string audience = this.GetConfigValue("MsalClientId");

            // Validate and decode the token
            this.ValidateJwtToken(idToken);

            // Fetch the JWKS JSON and await the result
            string jwksJson = await this.FetchJwksAsync(msalTenantId).ConfigureAwait(true);

            // Extract the "kid" (Key ID) from the JWT token header.
            string kid = this.GetKidFromToken(idToken);

            // Extract the first "x5c" (certificate chain) value for the specified "kid".
            string x5cKey = this.ExtractX5cFromJwks(jwksJson, kid);

            if (string.IsNullOrEmpty(x5cKey))
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} No matching x5c key found for the token's 'kid' in the JWKS.");
                return false;
            }

            // Get the RSA public key from the certificate
            RSA rsa = this.GetRsaPublicKeyFromCertificate(x5cKey);

            // Create the validation parameters
            TokenValidationParameters validationParameters = this.CreateValidationParameters(msalTenantId, audience, rsa);

            return this.ValidateTokenSignature(idToken, validationParameters);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} JWT validation failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Validates the provided JWT token and decodes it.
    /// </summary>
    /// <param name="idToken">The JWT token to be validated and decoded.</param>
    /// <returns>
    /// A <see cref="JwtSecurityToken"/> representing the decoded token.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the <paramref name="idToken"/> is null, empty, or not a valid JWT token.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when there is an error decoding the <paramref name="idToken"/>.
    /// </exception>
    private JwtSecurityToken ValidateJwtToken(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} The JWT id_token is null or empty.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        // Check if the token is in a readable JWT format
        if (!tokenHandler.CanReadToken(idToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} The id_token is not in a valid JWT format.");
        }

        // Decode the JWT token
        try
        {
            return tokenHandler.ReadJwtToken(idToken);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} Error decoding the id_token: " + ex.Message);
        }
    }

    /// <summary>
    /// Extracts the first "x5c" (certificate chain) value from the JSON Web Key Set (JWKS) JSON for the specified "kid".
    /// </summary>
    /// <param name="jwksJson">The JWKS JSON as a string.</param>
    /// <param name="kid">The Key ID ("kid") to search for in the JWKS.</param>
    /// <returns>
    /// The first "x5c" value as a string.
    /// Throws an <see cref="InvalidOperationException"/> if no matching "kid" or "x5c" is found.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no matching "kid" is found in the JWKS or the "x5c" value is missing.
    /// </exception>
    private string ExtractX5cFromJwks(string jwksJson, string kid)
    {
        using JsonDocument doc = JsonDocument.Parse(jwksJson);
        foreach (JsonElement key in doc.RootElement.GetProperty("keys").EnumerateArray())
        {
            if (key.TryGetProperty("kid", out JsonElement kidElement) && kidElement.GetString() == kid)
            {
                if (key.TryGetProperty("x5c", out JsonElement x5cElement) && x5cElement.GetArrayLength() > 0)
                {
                    return x5cElement[0].GetString()!;
                }
            }
        }

        throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The `x5c` value for the specified `kid` was not found in the JWKS.");
    }

    /// <summary>
    /// Extracts the "kid" (Key ID) from the JWT token header.
    /// </summary>
    /// <param name="token">The JWT token as a string.</param>
    /// <returns>
    /// The Key ID ("kid") value as a string, extracted from the token's header.
    /// Throws an <see cref="InvalidOperationException"/> if the "kid" claim is missing or cannot be found in the token header.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the "kid" claim is missing from the token header.
    /// </exception>
    private string GetKidFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        if (!jwtToken.Header.TryGetValue("kid", out object? kidValue) || kidValue == null)
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The `kid` claim is missing from the token header.");
        }

        return kidValue.ToString()!;
    }

    /// <summary>
    /// Creates an <see cref="X509Certificate2"/> object from a base64-encoded certificate string.
    /// </summary>
    /// <param name="base64Certificate">The base64-encoded string representing the certificate.</param>
    /// <returns>An <see cref="X509Certificate2"/> object representing the decoded certificate.</returns>
    /// <exception cref="FormatException">Thrown when the base64 string is invalid.</exception>
    private X509Certificate2 GetCertificateFromBase64(string base64Certificate)
    {
        byte[] certBytes = Convert.FromBase64String(base64Certificate);
        X509Certificate2 certificate = new X509Certificate2(certBytes);
        return certificate;
    }

    /// <summary>
    /// Extracts the RSA public key from the provided X509Certificate2.
    /// </summary>
    /// <param name="certificate">The X509Certificate2 from which to extract the RSA public key.</param>
    /// <returns>The extracted RSA public key.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the RSA public key could not be extracted from the certificate.
    /// </exception>
    private RSA GetRsaPublicKey(X509Certificate2 certificate)
    {
        RSA? rsa = certificate.GetRSAPublicKey();
        if (rsa == null)
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The RSA public key could not be extracted from the certificate.");
        }

        return rsa;
    }

    /// <summary>
    /// Retrieves a configuration value for the specified key and ensures it is not null or empty.
    /// </summary>
    /// <param name="key">The configuration key to look up.</param>
    /// <returns>The corresponding configuration value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the configuration value is missing or empty.
    /// </exception>
    /// <remarks>
    /// This method ensures that critical configuration values are present in the environment.
    /// If the value is missing, it raises an exception to alert the developer.
    /// </remarks>
    private string GetConfigValue(string key)
    {
        string? value = this.config[key];

        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} The `{key}` is missing from the environment variables.");
        }

        return value;
    }

    /// <summary>
    /// Fetches the JSON Web Key Set (JWKS) for the specified tenant ID.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant for which to fetch the JWKS.</param>
    /// <returns>A task representing the asynchronous operation, with the JWKS as a JSON string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the JWKS cannot be fetched due to an error.
    /// </exception>
    /// <remarks>
    /// This method retrieves the JWKS by delegating the call to the configured <see cref="IJwksFetcher"/>
    /// implementation and logs any errors that occur.
    /// </remarks>
    private async Task<string> FetchJwksAsync(string tenantId)
    {
        try
        {
            return await this.jwksFetcher.GetJwksAsync(tenantId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Failed to fetch JWKS for tenant {tenantId}: {ex.Message}");
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Unable to fetch JWKS for the given tenant.", ex);
        }
    }

    /// <summary>
    /// Extracts the RSA public key from an x5c certificate string.
    /// </summary>
    /// <param name="x5cKey">The Base64-encoded x5c certificate string.</param>
    /// <returns>The RSA public key extracted from the certificate.</returns>
    /// <exception cref="Exception">Thrown if the RSA key extraction fails.</exception>
    /// <remarks>
    /// The method converts the x5c certificate into an <see cref="X509Certificate2"/> object
    /// and retrieves the associated RSA public key.
    /// </remarks>
    private RSA GetRsaPublicKeyFromCertificate(string x5cKey)
    {
        try
        {
            X509Certificate2 certificate = this.GetCertificateFromBase64(x5cKey);
            return this.GetRsaPublicKey(certificate);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Failed to extract RSA public key from x5c: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates token validation parameters for verifying a JWT.
    /// </summary>
    /// <param name="tenantId">The tenant ID used to construct the expected issuer.</param>
    /// <param name="audience">The expected audience (client ID) of the token.</param>
    /// <param name="rsa">The RSA public key used to validate the token's signature.</param>
    /// <returns>
    /// A <see cref="TokenValidationParameters"/> object configured with the specified issuer, audience, and signing key.
    /// </returns>
    /// <remarks>
    /// The validation parameters include issuer, audience, signature, and lifetime validation.
    /// </remarks>
    private TokenValidationParameters CreateValidationParameters(string tenantId, string audience, RSA rsa)
    {
        string issuer = $"https://login.microsoftonline.com/{tenantId}/v2.0";

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Optional: prevent timing issues
        };
    }

    /// <summary>
    /// Validates the signature of a JWT using the provided validation parameters.
    /// </summary>
    /// <param name="idToken">The JWT to validate.</param>
    /// <param name="validationParameters">
    /// Parameters that define the validation rules, including the signing key, issuer, audience.
    /// </param>
    /// <returns>
    /// <c>true</c> if the token's signature is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="JwtSecurityTokenHandler.ValidateToken"/> to validate the JWT signature and claims.
    /// </remarks>
    private bool ValidateTokenSignature(string idToken, TokenValidationParameters validationParameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Validate the token's signature
            // This line ensures that the JWT signature is verified using the provided public key.
            // The `JwtSecurityTokenHandler.ValidateToken` method performs the following steps:
            // 1. Decodes the token into its header, payload, and signature components.
            // 2. Uses the provided RSA public key (`rsa`) to verify the signature's authenticity.
            // 3. Validates other claims like issuer, audience, and expiration time.
            tokenHandler.ValidateToken(idToken, validationParameters, out _);
            return true; // Valid signature
        }
        catch (SecurityTokenException ex)
        {
            this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} Token validation failed: {ex.Message}");
            return false; // Invalid signature
        }
    }
}
