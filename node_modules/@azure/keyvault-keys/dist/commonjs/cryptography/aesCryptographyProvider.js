var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
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
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
var aesCryptographyProvider_exports = {};
__export(aesCryptographyProvider_exports, {
  AesCryptographyProvider: () => AesCryptographyProvider
});
module.exports = __toCommonJS(aesCryptographyProvider_exports);
var import_node_crypto = __toESM(require("node:crypto"));
var import_models = require("./models.js");
class AesCryptographyProvider {
  key;
  constructor(key) {
    this.key = key;
  }
  encrypt(encryptParameters, _options) {
    const { algorithm, keySizeInBytes } = this.supportedAlgorithms[encryptParameters.algorithm];
    const iv = encryptParameters.iv || import_node_crypto.default.randomBytes(16);
    this.ensureValid(keySizeInBytes);
    const cipher = import_node_crypto.default.createCipheriv(algorithm, this.key.k.subarray(0, keySizeInBytes), iv);
    let encrypted = cipher.update(Buffer.from(encryptParameters.plaintext));
    encrypted = Buffer.concat([encrypted, cipher.final()]);
    return Promise.resolve({
      algorithm: encryptParameters.algorithm,
      result: encrypted,
      iv
    });
  }
  decrypt(decryptParameters, _options) {
    const { algorithm, keySizeInBytes } = this.supportedAlgorithms[decryptParameters.algorithm];
    this.ensureValid(keySizeInBytes);
    const decipher = import_node_crypto.default.createDecipheriv(
      algorithm,
      this.key.k.subarray(0, keySizeInBytes),
      decryptParameters.iv
    );
    let dec = decipher.update(Buffer.from(decryptParameters.ciphertext));
    dec = Buffer.concat([dec, decipher.final()]);
    return Promise.resolve({
      algorithm: decryptParameters.algorithm,
      result: dec
    });
  }
  isSupported(algorithm, operation) {
    if (!this.key.k) {
      return false;
    }
    if (!Object.keys(this.supportedAlgorithms).includes(algorithm)) {
      return false;
    }
    if (!this.supportedOperations.includes(operation)) {
      return false;
    }
    return true;
  }
  /**
   * The set of algorithms this provider supports.
   * For AES encryption, the values include the underlying algorithm used in crypto
   * as well as the key size in bytes.
   *
   * We start with support for A[SIZE]CBCPAD which uses the PKCS padding (the default padding scheme in node crypto)
   */
  supportedAlgorithms = {
    A128CBCPAD: {
      algorithm: "aes-128-cbc",
      keySizeInBytes: 128 >> 3
    },
    A192CBCPAD: {
      algorithm: "aes-192-cbc",
      keySizeInBytes: 192 >> 3
    },
    A256CBCPAD: {
      algorithm: "aes-256-cbc",
      keySizeInBytes: 256 >> 3
    }
  };
  supportedOperations = ["encrypt", "decrypt"];
  wrapKey(_algorithm, _keyToWrap, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Wrapping a key using a local JsonWebKey is not supported for AES."
    );
  }
  unwrapKey(_algorithm, _encryptedKey, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Unwrapping a key using a local JsonWebKey is not supported for AES."
    );
  }
  sign(_algorithm, _digest, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Signing using a local JsonWebKey is not supported for AES."
    );
  }
  signData(_algorithm, _data, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Signing using a local JsonWebKey is not supported for AES."
    );
  }
  verify(_algorithm, _digest, _signature, _options) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Verifying using a local JsonWebKey is not supported for AES."
    );
  }
  verifyData(_algorithm, _data, _signature, _updatedOptions) {
    throw new import_models.LocalCryptographyUnsupportedError(
      "Verifying using a local JsonWebKey is not supported for AES."
    );
  }
  ensureValid(keySizeInBytes) {
    if (this.key && this.key.kty?.toUpperCase() !== "OCT" && this.key.kty?.toUpperCase() !== "OCT-HSM") {
      throw new Error("Key type does not match the key type oct or oct-hsm");
    }
    if (!this.key.k) {
      throw new Error("Symmetric key is required");
    }
    if (this.key.k.length < keySizeInBytes) {
      throw new Error(`Key must be at least ${keySizeInBytes << 3} bits`);
    }
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  AesCryptographyProvider
});
//# sourceMappingURL=aesCryptographyProvider.js.map
