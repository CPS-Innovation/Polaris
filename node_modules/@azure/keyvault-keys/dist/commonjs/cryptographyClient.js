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
var cryptographyClient_exports = {};
__export(cryptographyClient_exports, {
  CryptographyClient: () => CryptographyClient
});
module.exports = __toCommonJS(cryptographyClient_exports);
var import_keysModels = require("./keysModels.js");
var import_remoteCryptographyProvider = require("./cryptography/remoteCryptographyProvider.js");
var import_crypto = require("./cryptography/crypto.js");
var import_rsaCryptographyProvider = require("./cryptography/rsaCryptographyProvider.js");
var import_aesCryptographyProvider = require("./cryptography/aesCryptographyProvider.js");
var import_tracing = require("./tracing.js");
var import_core_rest_pipeline = require("@azure/core-rest-pipeline");
var import_logger = require("./logger.js");
class CryptographyClient {
  /**
   * The key the CryptographyClient currently holds.
   */
  key;
  /**
   * The remote provider, which would be undefined if used in local mode.
   */
  remoteProvider;
  /**
   * Internal constructor implementation for either local or Key Vault backed keys.
   * @param key - The key to use during cryptography tasks.
   * @param credential - Teh credential to use when constructing a Key Vault Cryptography client.
   */
  constructor(key, credential, pipelineOptions = {}) {
    if (typeof key === "string") {
      this.key = {
        kind: "identifier",
        value: key
      };
      this.remoteProvider = new import_remoteCryptographyProvider.RemoteCryptographyProvider(key, credential, pipelineOptions);
    } else if ("name" in key) {
      this.key = {
        kind: "KeyVaultKey",
        value: key
      };
      this.remoteProvider = new import_remoteCryptographyProvider.RemoteCryptographyProvider(key, credential, pipelineOptions);
    } else {
      this.key = {
        kind: "JsonWebKey",
        value: key
      };
    }
  }
  /**
   * The base URL to the vault. If a local {@link JsonWebKey} is used vaultUrl will be empty.
   */
  get vaultUrl() {
    return this.remoteProvider?.vaultUrl || "";
  }
  /**
   * The ID of the key used to perform cryptographic operations for the client.
   */
  get keyID() {
    if (this.key.kind === "identifier" || this.key.kind === "remoteOnlyIdentifier") {
      return this.key.value;
    } else if (this.key.kind === "KeyVaultKey") {
      return this.key.value.id;
    } else {
      return this.key.value.kid;
    }
  }
  encrypt(...args) {
    const [parameters, options] = this.disambiguateEncryptArguments(args);
    return import_tracing.tracingClient.withSpan("CryptographyClient.encrypt", options, async (updatedOptions) => {
      this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Encrypt);
      this.initializeIV(parameters);
      const provider = await this.getProvider("encrypt", parameters.algorithm, updatedOptions);
      try {
        return provider.encrypt(parameters, updatedOptions);
      } catch (error) {
        if (this.remoteProvider) {
          return this.remoteProvider.encrypt(parameters, updatedOptions);
        }
        throw error;
      }
    });
  }
  initializeIV(parameters) {
    const algorithmsRequiringIV = [
      "A128CBC",
      "A128CBCPAD",
      "A192CBC",
      "A192CBCPAD",
      "A256CBC",
      "A256CBCPAD"
    ];
    if (parameters.algorithm in algorithmsRequiringIV) {
      try {
        const cbcParams = parameters;
        if (!cbcParams.iv) {
          cbcParams.iv = (0, import_crypto.randomBytes)(16);
        }
      } catch (e) {
        throw new Error(
          `Unable to initialize IV for algorithm ${parameters.algorithm}. You may pass a valid IV to avoid this error. Error: ${e.message}`
        );
      }
    }
  }
  /**
   * Standardizes the arguments of multiple overloads into a single shape.
   * @param args - The encrypt arguments
   */
  disambiguateEncryptArguments(args) {
    if (typeof args[0] === "string") {
      return [
        {
          algorithm: args[0],
          plaintext: args[1]
        },
        args[2] || {}
      ];
    } else {
      return [args[0], args[1] || {}];
    }
  }
  decrypt(...args) {
    const [parameters, options] = this.disambiguateDecryptArguments(args);
    return import_tracing.tracingClient.withSpan("CryptographyClient.decrypt", options, async (updatedOptions) => {
      this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Decrypt);
      const provider = await this.getProvider("decrypt", parameters.algorithm, updatedOptions);
      try {
        return provider.decrypt(parameters, updatedOptions);
      } catch (error) {
        if (this.remoteProvider) {
          return this.remoteProvider.decrypt(parameters, updatedOptions);
        }
        throw error;
      }
    });
  }
  /**
   * Standardizes the arguments of multiple overloads into a single shape.
   * @param args - The decrypt arguments
   */
  disambiguateDecryptArguments(args) {
    if (typeof args[0] === "string") {
      return [
        {
          algorithm: args[0],
          ciphertext: args[1]
        },
        args[2] || {}
      ];
    } else {
      return [args[0], args[1] || {}];
    }
  }
  /**
   * Wraps the given key using the specified cryptography algorithm
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleWrapKey
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * const myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const wrapResult = await cryptographyClient.wrapKey("RSA-OAEP", Buffer.from("My Key"));
   * console.log("wrap result:", wrapResult.result);
   * ```
   * @param algorithm - The encryption algorithm to use to wrap the given key.
   * @param key - The key to wrap.
   * @param options - Additional options.
   */
  wrapKey(algorithm, key, options = {}) {
    return import_tracing.tracingClient.withSpan("CryptographyClient.wrapKey", options, async (updatedOptions) => {
      this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.WrapKey);
      const provider = await this.getProvider("wrapKey", algorithm, updatedOptions);
      try {
        return provider.wrapKey(algorithm, key, updatedOptions);
      } catch (err) {
        if (this.remoteProvider) {
          return this.remoteProvider.wrapKey(algorithm, key, options);
        }
        throw err;
      }
    });
  }
  /**
   * Unwraps the given wrapped key using the specified cryptography algorithm
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleUnwrapKey
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * const myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const wrapResult = await cryptographyClient.wrapKey("RSA-OAEP", Buffer.from("My Key"));
   * console.log("wrap result:", wrapResult.result);
   *
   * const unwrapResult = await cryptographyClient.unwrapKey("RSA-OAEP", wrapResult.result);
   * console.log("unwrap result: ", unwrapResult.result);
   * ```
   * @param algorithm - The decryption algorithm to use to unwrap the key.
   * @param encryptedKey - The encrypted key to unwrap.
   * @param options - Additional options.
   */
  unwrapKey(algorithm, encryptedKey, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "CryptographyClient.unwrapKey",
      options,
      async (updatedOptions) => {
        this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.UnwrapKey);
        const provider = await this.getProvider("unwrapKey", algorithm, updatedOptions);
        try {
          return provider.unwrapKey(algorithm, encryptedKey, updatedOptions);
        } catch (err) {
          if (this.remoteProvider) {
            return this.remoteProvider.unwrapKey(algorithm, encryptedKey, options);
          }
          throw err;
        }
      }
    );
  }
  /**
   * Cryptographically sign the digest of a message
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleSign
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   * import { createHash } from "node:crypto";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * let myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const signatureValue = "MySignature";
   * const hash = createHash("sha256");
   *
   * const digest = hash.update(signatureValue).digest();
   * console.log("digest: ", digest);
   *
   * const signResult = await cryptographyClient.sign("RS256", digest);
   * console.log("sign result: ", signResult.result);
   * ```
   * @param algorithm - The signing algorithm to use.
   * @param digest - The digest of the data to sign.
   * @param options - Additional options.
   */
  sign(algorithm, digest, options = {}) {
    return import_tracing.tracingClient.withSpan("CryptographyClient.sign", options, async (updatedOptions) => {
      this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Sign);
      const provider = await this.getProvider("sign", algorithm, updatedOptions);
      try {
        return provider.sign(algorithm, digest, updatedOptions);
      } catch (err) {
        if (this.remoteProvider) {
          return this.remoteProvider.sign(algorithm, digest, updatedOptions);
        }
        throw err;
      }
    });
  }
  /**
   * Verify the signed message digest
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleVerify
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   * import { createHash } from "node:crypto";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * const myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const hash = createHash("sha256");
   * hash.update("My Message");
   * const digest = hash.digest();
   *
   * const signResult = await cryptographyClient.sign("RS256", digest);
   * console.log("sign result: ", signResult.result);
   *
   * const verifyResult = await cryptographyClient.verify("RS256", digest, signResult.result);
   * console.log("verify result: ", verifyResult.result);
   * ```
   * @param algorithm - The signing algorithm to use to verify with.
   * @param digest - The digest to verify.
   * @param signature - The signature to verify the digest against.
   * @param options - Additional options.
   */
  verify(algorithm, digest, signature, options = {}) {
    return import_tracing.tracingClient.withSpan("CryptographyClient.verify", options, async (updatedOptions) => {
      this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Verify);
      const provider = await this.getProvider("verify", algorithm, updatedOptions);
      try {
        return provider.verify(algorithm, digest, signature, updatedOptions);
      } catch (err) {
        if (this.remoteProvider) {
          return this.remoteProvider.verify(algorithm, digest, signature, updatedOptions);
        }
        throw err;
      }
    });
  }
  /**
   * Cryptographically sign a block of data
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleSignData
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * const myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const signResult = await cryptographyClient.signData("RS256", Buffer.from("My Message"));
   * console.log("sign result: ", signResult.result);
   * ```
   * @param algorithm - The signing algorithm to use.
   * @param data - The data to sign.
   * @param options - Additional options.
   */
  signData(algorithm, data, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "CryptographyClient.signData",
      options,
      async (updatedOptions) => {
        this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Sign);
        const provider = await this.getProvider("signData", algorithm, updatedOptions);
        try {
          return provider.signData(algorithm, data, updatedOptions);
        } catch (err) {
          if (this.remoteProvider) {
            return this.remoteProvider.signData(algorithm, data, options);
          }
          throw err;
        }
      }
    );
  }
  /**
   * Verify the signed block of data
   *
   * Example usage:
   * ```ts snippet:ReadmeSampleVerifyData
   * import { DefaultAzureCredential } from "@azure/identity";
   * import { KeyClient, CryptographyClient } from "@azure/keyvault-keys";
   *
   * const credential = new DefaultAzureCredential();
   *
   * const vaultName = "<YOUR KEYVAULT NAME>";
   * const url = `https://${vaultName}.vault.azure.net`;
   *
   * const client = new KeyClient(url, credential);
   *
   * const myKey = await client.createKey("MyKey", "RSA");
   * const cryptographyClient = new CryptographyClient(myKey, credential);
   *
   * const buffer = Buffer.from("My Message");
   *
   * const signResult = await cryptographyClient.signData("RS256", buffer);
   * console.log("sign result: ", signResult.result);
   *
   * const verifyResult = await cryptographyClient.verifyData("RS256", buffer, signResult.result);
   * console.log("verify result: ", verifyResult.result);
   * ```
   * @param algorithm - The algorithm to use to verify with.
   * @param data - The signed block of data to verify.
   * @param signature - The signature to verify the block against.
   * @param options - Additional options.
   */
  verifyData(algorithm, data, signature, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "CryptographyClient.verifyData",
      options,
      async (updatedOptions) => {
        this.ensureValid(await this.fetchKey(updatedOptions), import_keysModels.KnownKeyOperations.Verify);
        const provider = await this.getProvider("verifyData", algorithm, updatedOptions);
        try {
          return provider.verifyData(algorithm, data, signature, updatedOptions);
        } catch (err) {
          if (this.remoteProvider) {
            return this.remoteProvider.verifyData(algorithm, data, signature, updatedOptions);
          }
          throw err;
        }
      }
    );
  }
  /**
   * Retrieves the {@link JsonWebKey} from the Key Vault, if possible. Returns undefined if the key could not be retrieved due to insufficient permissions.
   * @param options - The additional options.
   */
  async getKeyMaterial(options) {
    const key = await this.fetchKey(options);
    switch (key.kind) {
      case "JsonWebKey":
        return key.value;
      case "KeyVaultKey":
        return key.value.key;
      default:
        return void 0;
    }
  }
  /**
   * Returns the underlying key used for cryptographic operations.
   * If needed, attempts to fetch the key from KeyVault and exchanges the ID for the actual key.
   * @param options - The additional options.
   */
  async fetchKey(options) {
    if (this.key.kind === "identifier") {
      let key;
      try {
        key = await this.remoteProvider.getKey(options);
      } catch (e) {
        if ((0, import_core_rest_pipeline.isRestError)(e) && e.statusCode === 403) {
          import_logger.logger.verbose(
            `Permission denied to get key ${this.key.value}. Falling back to remote operation.`
          );
          this.key = { kind: "remoteOnlyIdentifier", value: this.key.value };
        } else {
          throw e;
        }
      }
      if (key) {
        this.key = { kind: "KeyVaultKey", value: key };
      }
    }
    return this.key;
  }
  providers;
  /**
   * Gets the provider that support this algorithm and operation.
   * The available providers are ordered by priority such that the first provider that supports this
   * operation is the one we should use.
   * @param operation - The {@link KeyOperation}.
   * @param algorithm - The algorithm to use.
   */
  async getProvider(operation, algorithm, options) {
    if (!this.providers) {
      const keyMaterial = await this.getKeyMaterial(options);
      this.providers = [];
      if (keyMaterial) {
        this.providers.push(
          new import_rsaCryptographyProvider.RsaCryptographyProvider(keyMaterial),
          new import_aesCryptographyProvider.AesCryptographyProvider(keyMaterial)
        );
      }
      if (this.remoteProvider) {
        this.providers.push(this.remoteProvider);
      }
    }
    const providers = this.providers.filter((p) => p.isSupported(algorithm, operation));
    if (providers.length === 0) {
      throw new Error(
        `Unable to support operation: "${operation}" with algorithm: "${algorithm}" ${this.key.kind === "JsonWebKey" ? "using a local JsonWebKey" : ""}`
      );
    }
    return providers[0];
  }
  ensureValid(key, operation) {
    if (key.kind === "KeyVaultKey") {
      const keyOps = key.value.keyOperations;
      const { notBefore, expiresOn } = key.value.properties;
      const now = /* @__PURE__ */ new Date();
      if (notBefore && now < notBefore) {
        throw new Error(`Key ${key.value.id} can't be used before ${notBefore.toISOString()}`);
      }
      if (expiresOn && now > expiresOn) {
        throw new Error(`Key ${key.value.id} expired at ${expiresOn.toISOString()}`);
      }
      if (operation && keyOps && !keyOps?.includes(operation)) {
        throw new Error(`Operation ${operation} is not supported on key ${key.value.id}`);
      }
    } else if (key.kind === "JsonWebKey") {
      if (operation && key.value.keyOps && !key.value.keyOps?.includes(operation)) {
        throw new Error(`Operation ${operation} is not supported on key ${key.value.kid}`);
      }
    }
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  CryptographyClient
});
//# sourceMappingURL=cryptographyClient.js.map
