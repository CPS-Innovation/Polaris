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
var rsaCryptographyProvider_exports = {};
__export(rsaCryptographyProvider_exports, {
  RsaCryptographyProvider: () => RsaCryptographyProvider
});
module.exports = __toCommonJS(rsaCryptographyProvider_exports);
var import_constants = require("constants");
var import_node_crypto = require("node:crypto");
var import_crypto = require("./crypto.js");
var import_conversions = require("./conversions.js");
var import_models = require("./models.js");
class RsaCryptographyProvider {
  constructor(key) {
    this.key = key;
  }
  isSupported(algorithm, operation) {
    return this.applicableAlgorithms.includes(algorithm) && this.applicableOperations.includes(operation);
  }
  encrypt(encryptParameters, _options) {
    this.ensureValid();
    const keyPEM = (0, import_conversions.convertJWKtoPEM)(this.key);
    const padding = encryptParameters.algorithm === "RSA1_5" ? import_constants.RSA_PKCS1_PADDING : import_constants.RSA_PKCS1_OAEP_PADDING;
    return Promise.resolve({
      algorithm: encryptParameters.algorithm,
      keyID: this.key.kid,
      result: (0, import_node_crypto.publicEncrypt)(
        { key: keyPEM, padding },
        Buffer.from(encryptParameters.plaintext)
      )
    });
  }
  decrypt(_decryptParameters, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Decrypting using a local JsonWebKey is not supported."
    );
  }
  wrapKey(algorithm, keyToWrap, _options) {
    this.ensureValid();
    const keyPEM = (0, import_conversions.convertJWKtoPEM)(this.key);
    const padding = algorithm === "RSA1_5" ? import_constants.RSA_PKCS1_PADDING : import_constants.RSA_PKCS1_OAEP_PADDING;
    return Promise.resolve({
      algorithm,
      result: (0, import_node_crypto.publicEncrypt)({ key: keyPEM, padding }, Buffer.from(keyToWrap)),
      keyID: this.key.kid
    });
  }
  unwrapKey(_algorithm, _encryptedKey, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Unwrapping a key using a local JsonWebKey is not supported."
    );
  }
  sign(_algorithm, _digest, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Signing a digest using a local JsonWebKey is not supported."
    );
  }
  signData(_algorithm, _data, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Signing a block of data using a local JsonWebKey is not supported."
    );
  }
  async verify(_algorithm, _digest, _signature, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Verifying a digest using a local JsonWebKey is not supported."
    );
  }
  verifyData(algorithm, data, signature, _options) {
    this.ensureValid();
    const keyPEM = (0, import_conversions.convertJWKtoPEM)(this.key);
    const verifier = (0, import_crypto.createVerify)(algorithm, data);
    return Promise.resolve({
      result: verifier.verify(keyPEM, Buffer.from(signature)),
      keyID: this.key.kid
    });
  }
  /**
   * The {@link JsonWebKey} used to perform crypto operations.
   */
  key;
  /**
   * The set of algorithms this provider supports
   */
  applicableAlgorithms = [
    "RSA1_5",
    "RSA-OAEP",
    "PS256",
    "RS256",
    "PS384",
    "RS384",
    "PS512",
    "RS512"
  ];
  /**
   * The set of operations this provider supports
   */
  applicableOperations = [
    "encrypt",
    "wrapKey",
    "verifyData"
  ];
  /**
   * Mapping between signature algorithms and their corresponding hash algorithms. Externally used for testing.
   * @internal
   */
  signatureAlgorithmToHashAlgorithm = {
    PS256: "SHA256",
    RS256: "SHA256",
    PS384: "SHA384",
    RS384: "SHA384",
    PS512: "SHA512",
    RS512: "SHA512"
  };
  ensureValid() {
    if (this.key && this.key.kty?.toUpperCase() !== "RSA" && this.key.kty?.toUpperCase() !== "RSA-HSM") {
      throw new Error("Key type does not match the algorithm RSA");
    }
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  RsaCryptographyProvider
});
//# sourceMappingURL=rsaCryptographyProvider.js.map
