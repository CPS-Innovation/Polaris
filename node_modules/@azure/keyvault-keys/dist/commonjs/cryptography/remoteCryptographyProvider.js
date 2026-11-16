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
var remoteCryptographyProvider_exports = {};
__export(remoteCryptographyProvider_exports, {
  RemoteCryptographyProvider: () => RemoteCryptographyProvider
});
module.exports = __toCommonJS(remoteCryptographyProvider_exports);
var import_constants = require("../constants.js");
var import_keyVaultClient = require("../keyVaultClient.js");
var import_identifier = require("../identifier.js");
var import_keysModels = require("../keysModels.js");
var import_transformations = require("../transformations.js");
var import_crypto = require("./crypto.js");
var import_logger = require("../logger.js");
var import_keyvault_common = require("@azure/keyvault-common");
var import_tracing = require("../tracing.js");
var import_core_rest_pipeline = require("@azure/core-rest-pipeline");
class RemoteCryptographyProvider {
  constructor(key, credential, pipelineOptions = {}) {
    this.key = key;
    let keyId;
    if (typeof key === "string") {
      keyId = key;
    } else {
      keyId = key.id;
    }
    try {
      const parsed = (0, import_identifier.parseKeyVaultKeyIdentifier)(keyId);
      if (parsed.name === "") {
        throw new Error("Could not find 'name' of key in key URL");
      }
      if (!parsed.vaultUrl || parsed.vaultUrl === "") {
        throw new Error("Could not find 'vaultUrl' of key in key URL");
      }
      this.vaultUrl = parsed.vaultUrl;
      this.name = parsed.name;
      this.version = parsed.version ?? "";
      this.client = getOrInitializeClient(this.vaultUrl, credential, pipelineOptions);
    } catch (err) {
      import_logger.logger.error(err);
      throw new Error(`${keyId} is not a valid Key Vault key ID`);
    }
  }
  // The remote client supports all algorithms and all operations.
  isSupported(_algorithm, _operation) {
    return true;
  }
  encrypt(encryptParameters, options = {}) {
    const { algorithm, plaintext, ...params } = encryptParameters;
    const requestOptions = { ...options, ...params };
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.encrypt",
      requestOptions,
      async (updatedOptions) => {
        const result = await this.client.encrypt(
          this.name,
          this.version,
          {
            algorithm,
            value: plaintext,
            aad: "additionalAuthenticatedData" in encryptParameters ? encryptParameters.additionalAuthenticatedData : void 0,
            iv: "iv" in encryptParameters ? encryptParameters.iv : void 0
          },
          updatedOptions
        );
        return {
          algorithm: encryptParameters.algorithm,
          result: result.result,
          keyID: this.getKeyID(),
          additionalAuthenticatedData: result.additionalAuthenticatedData,
          authenticationTag: result.authenticationTag,
          iv: result.iv
        };
      }
    );
  }
  decrypt(decryptParameters, options = {}) {
    const { algorithm, ciphertext, ...params } = decryptParameters;
    const requestOptions = { ...options, ...params };
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.decrypt",
      requestOptions,
      async (updatedOptions) => {
        const result = await this.client.decrypt(
          this.name,
          this.version,
          {
            algorithm,
            value: ciphertext,
            aad: "additionalAuthenticatedData" in decryptParameters ? decryptParameters.additionalAuthenticatedData : void 0,
            iv: "iv" in decryptParameters ? decryptParameters.iv : void 0,
            tag: "authenticationTag" in decryptParameters ? decryptParameters.authenticationTag : void 0
          },
          updatedOptions
        );
        return {
          result: result.result,
          keyID: this.getKeyID(),
          algorithm
        };
      }
    );
  }
  wrapKey(algorithm, keyToWrap, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.wrapKey",
      options,
      async (updatedOptions) => {
        const result = await this.client.wrapKey(
          this.name,
          this.version,
          {
            algorithm,
            value: keyToWrap
          },
          updatedOptions
        );
        return {
          result: result.result,
          algorithm,
          keyID: this.getKeyID()
        };
      }
    );
  }
  unwrapKey(algorithm, encryptedKey, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.unwrapKey",
      options,
      async (updatedOptions) => {
        const result = await this.client.unwrapKey(
          this.name,
          this.version,
          {
            algorithm,
            value: encryptedKey
          },
          updatedOptions
        );
        return {
          result: result.result,
          algorithm,
          keyID: this.getKeyID()
        };
      }
    );
  }
  sign(algorithm, digest, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.sign",
      options,
      async (updatedOptions) => {
        const result = await this.client.sign(
          this.name,
          this.version,
          {
            algorithm,
            value: digest
          },
          updatedOptions
        );
        return { result: result.result, algorithm, keyID: this.getKeyID() };
      }
    );
  }
  verifyData(algorithm, data, signature, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.verifyData",
      options,
      async (updatedOptions) => {
        const hash = await (0, import_crypto.createHash)(algorithm, data);
        return this.verify(algorithm, hash, signature, updatedOptions);
      }
    );
  }
  verify(algorithm, digest, signature, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.verify",
      options,
      async (updatedOptions) => {
        const response = await this.client.verify(
          this.name,
          this.version,
          {
            algorithm,
            digest,
            signature
          },
          updatedOptions
        );
        return {
          result: response.value ? response.value : false,
          keyID: this.getKeyID()
        };
      }
    );
  }
  signData(algorithm, data, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.signData",
      options,
      async (updatedOptions) => {
        const digest = await (0, import_crypto.createHash)(algorithm, data);
        const result = await this.client.sign(
          this.name,
          this.version,
          {
            algorithm,
            value: digest
          },
          updatedOptions
        );
        return { result: result.result, algorithm, keyID: this.getKeyID() };
      }
    );
  }
  /**
   * The base URL to the vault.
   */
  vaultUrl;
  /**
   * The ID of the key used to perform cryptographic operations for the client.
   */
  get keyId() {
    return this.getKeyID();
  }
  /**
   * Gets the {@link KeyVaultKey} used for cryptography operations, fetching it
   * from KeyVault if necessary.
   * @param options - Additional options.
   */
  getKey(options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RemoteCryptographyProvider.getKey",
      options,
      async (updatedOptions) => {
        if (typeof this.key === "string") {
          if (!this.name || this.name === "") {
            throw new Error("getKey requires a key with a name");
          }
          const response = await this.client.getKey(
            this.name,
            options && options.version ? options.version : this.version ? this.version : "",
            updatedOptions
          );
          this.key = (0, import_transformations.getKeyFromKeyBundle)(response);
        }
        return this.key;
      }
    );
  }
  /**
   * A reference to the auto-generated KeyVault HTTP client.
   */
  client;
  /**
   * A reference to the key used for the cryptographic operations.
   * Based on what was provided to the CryptographyClient constructor,
   * it can be either a string with the URL of a Key Vault Key, or an already parsed {@link KeyVaultKey}.
   */
  key;
  /**
   * Name of the key the client represents
   */
  name;
  /**
   * Version of the key the client represents
   */
  version;
  /**
   * Attempts to retrieve the ID of the key.
   */
  getKeyID() {
    let kid;
    if (typeof this.key !== "string") {
      kid = this.key.id;
    } else {
      kid = this.key;
    }
    return kid;
  }
}
function getOrInitializeClient(vaultUrl, credential, options) {
  if (options.generatedClient) {
    return options.generatedClient;
  }
  const libInfo = `azsdk-js-keyvault-keys/${import_constants.SDK_VERSION}`;
  const userAgentOptions = options.userAgentOptions;
  options.userAgentOptions = {
    userAgentPrefix: userAgentOptions && userAgentOptions.userAgentPrefix ? `${userAgentOptions.userAgentPrefix} ${libInfo}` : libInfo
  };
  const internalPipelineOptions = {
    ...options,
    apiVersion: options.serviceVersion || import_keysModels.LATEST_API_VERSION,
    loggingOptions: {
      logger: import_logger.logger.info,
      additionalAllowedHeaderNames: [
        "x-ms-keyvault-region",
        "x-ms-keyvault-network-info",
        "x-ms-keyvault-service-version"
      ]
    }
  };
  const client = new import_keyVaultClient.KeyVaultClient(vaultUrl, credential, internalPipelineOptions);
  client.pipeline.removePolicy({ name: import_core_rest_pipeline.bearerTokenAuthenticationPolicyName });
  client.pipeline.addPolicy((0, import_keyvault_common.keyVaultAuthenticationPolicy)(credential, options));
  client.pipeline.addPolicy({
    name: "ContentTypePolicy",
    sendRequest(request, next) {
      const contentType = request.headers.get("Content-Type") ?? "";
      if (contentType.startsWith("application/json")) {
        request.headers.set("Content-Type", "application/json");
      }
      return next(request);
    }
  });
  return client;
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  RemoteCryptographyProvider
});
//# sourceMappingURL=remoteCryptographyProvider.js.map
