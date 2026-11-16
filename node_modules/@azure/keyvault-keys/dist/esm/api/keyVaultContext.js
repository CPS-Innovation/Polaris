// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import { logger } from "../logger.js";
import { getClient } from "@azure-rest/core-client";
import { SDK_VERSION } from "../constants.js";
/** The key vault client performs cryptographic key operations and vault operations against the Key Vault service. */
export function createKeyVault(endpointParam, credential, options = {}) {
    const endpointUrl = options.endpoint ?? options.baseUrl ?? String(endpointParam);
    const prefixFromOptions = options?.userAgentOptions?.userAgentPrefix;
    const userAgentInfo = `azsdk-js-keyvault-keys/${SDK_VERSION}`;
    const userAgentPrefix = prefixFromOptions
        ? `${prefixFromOptions} azsdk-js-api ${userAgentInfo}`
        : `azsdk-js-api ${userAgentInfo}`;
    const { apiVersion: _, ...updatedOptions } = {
        ...options,
        userAgentOptions: { userAgentPrefix },
        loggingOptions: { logger: options.loggingOptions?.logger ?? logger.info },
        credentials: {
            scopes: options.credentials?.scopes ?? ["https://vault.azure.net/.default"],
        },
    };
    const clientContext = getClient(endpointUrl, credential, updatedOptions);
    clientContext.pipeline.removePolicy({ name: "ApiVersionPolicy" });
    const apiVersion = options.apiVersion ?? "2025-07-01";
    clientContext.pipeline.addPolicy({
        name: "ClientApiVersionPolicy",
        sendRequest: (req, next) => {
            // Use the apiVersion defined in request url directly
            // Append one if there is no apiVersion and we have one at client options
            const url = new URL(req.url);
            if (!url.searchParams.get("api-version")) {
                req.url = `${req.url}${Array.from(url.searchParams.keys()).length > 0 ? "&" : "?"}api-version=${apiVersion}`;
            }
            return next(req);
        },
    });
    return { ...clientContext, apiVersion };
}
//# sourceMappingURL=keyVaultContext.js.map