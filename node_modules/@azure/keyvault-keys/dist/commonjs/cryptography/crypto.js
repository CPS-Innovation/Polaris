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
var crypto_exports = {};
__export(crypto_exports, {
  createHash: () => createHash,
  createVerify: () => createVerify,
  randomBytes: () => randomBytes
});
module.exports = __toCommonJS(crypto_exports);
var import_node_crypto = require("node:crypto");
const algorithmToHashAlgorithm = {
  ES256: "SHA256",
  ES256K: "SHA256",
  PS256: "SHA256",
  RS256: "SHA256",
  ES384: "SHA384",
  PS384: "SHA384",
  RS384: "SHA384",
  ES512: "SHA512",
  PS512: "SHA512",
  RS512: "SHA512"
};
async function createHash(algorithm, data) {
  const hashAlgorithm = algorithmToHashAlgorithm[algorithm];
  if (!hashAlgorithm) {
    throw new Error(
      `Invalid algorithm ${algorithm} passed to createHash. Supported algorithms: ${Object.keys(
        algorithmToHashAlgorithm
      ).join(", ")}`
    );
  }
  const hash = (0, import_node_crypto.createHash)(hashAlgorithm);
  hash.update(Buffer.from(data));
  const digest = hash.digest();
  return digest;
}
function createVerify(algorithm, data) {
  const verifyAlgorithm = algorithmToHashAlgorithm[algorithm];
  if (!verifyAlgorithm) {
    throw new Error(
      `Invalid algorithm ${algorithm} passed to createHash. Supported algorithms: ${Object.keys(
        algorithmToHashAlgorithm
      ).join(", ")}`
    );
  }
  const verifier = (0, import_node_crypto.createVerify)(verifyAlgorithm);
  verifier.update(Buffer.from(data));
  verifier.end();
  return verifier;
}
function randomBytes(length) {
  return (0, import_node_crypto.randomBytes)(length);
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  createHash,
  createVerify,
  randomBytes
});
//# sourceMappingURL=crypto.js.map
