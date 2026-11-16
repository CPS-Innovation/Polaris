var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
var keyVaultContext_exports = {};
__export(keyVaultContext_exports, {
  createKeyVault: () => createKeyVault
});
module.exports = __toCommonJS(keyVaultContext_exports);
var import_logger = require("../logger.js");
var import_core_client = require("@azure-rest/core-client");
var import_constants = require("../constants.js");
function createKeyVault(endpointParam, credential, options = {}) {
  const endpointUrl = options.endpoint ?? options.baseUrl ?? String(endpointParam);
  const prefixFromOptions = options?.userAgentOptions?.userAgentPrefix;
  const userAgentInfo = `azsdk-js-keyvault-keys/${import_constants.SDK_VERSION}`;
  const userAgentPrefix = prefixFromOptions ? `${prefixFromOptions} azsdk-js-api ${userAgentInfo}` : `azsdk-js-api ${userAgentInfo}`;
  const { apiVersion: _, ...updatedOptions } = {
    ...options,
    userAgentOptions: { userAgentPrefix },
    loggingOptions: { logger: options.loggingOptions?.logger ?? import_logger.logger.info },
    credentials: {
      scopes: options.credentials?.scopes ?? ["https://vault.azure.net/.default"]
    }
  };
  const clientContext = (0, import_core_client.getClient)(endpointUrl, credential, updatedOptions);
  clientContext.pipeline.removePolicy({ name: "ApiVersionPolicy" });
  const apiVersion = options.apiVersion ?? "2025-07-01";
  clientContext.pipeline.addPolicy({
    name: "ClientApiVersionPolicy",
    sendRequest: (req, next) => {
      const url = new URL(req.url);
      if (!url.searchParams.get("api-version")) {
        req.url = `${req.url}${Array.from(url.searchParams.keys()).length > 0 ? "&" : "?"}api-version=${apiVersion}`;
      }
      return next(req);
    }
  });
  return { ...clientContext, apiVersion };
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  createKeyVault
});
//# sourceMappingURL=keyVaultContext.js.map
