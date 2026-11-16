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
var conversions_exports = {};
__export(conversions_exports, {
  convertJWKtoPEM: () => convertJWKtoPEM
});
module.exports = __toCommonJS(conversions_exports);
var import_core_util = require("@azure/core-util");
function encodeLength(length) {
  if (length <= 127) {
    return Uint8Array.of(length);
  } else if (length < 256) {
    return Uint8Array.of(129, length);
  } else if (length < 65536) {
    return Uint8Array.of(130, length >> 8, length & 255);
  } else {
    throw new Error("Unsupported length to encode");
  }
}
function encodeBuffer(buffer, bufferId) {
  if (buffer.length === 0) {
    return buffer;
  }
  let result = new Uint8Array(buffer);
  if (result[0] & 128) {
    const array = new Uint8Array(result.length + 1);
    array[0] = 0;
    array.set(result, 1);
    result = array;
  }
  const encodedLength = encodeLength(result.length);
  const totalLength = 1 + encodedLength.length + result.length;
  const outputBuffer = new Uint8Array(totalLength);
  outputBuffer[0] = bufferId;
  outputBuffer.set(encodedLength, 1);
  outputBuffer.set(result, 1 + encodedLength.length);
  return outputBuffer;
}
function makeSequence(encodedParts) {
  const totalLength = encodedParts.reduce((sum, part) => sum + part.length, 0);
  const sequence = new Uint8Array(totalLength);
  for (let i = 0; i < encodedParts.length; i++) {
    const previousLength = i > 0 ? encodedParts[i - 1].length : 0;
    sequence.set(encodedParts[i], previousLength);
  }
  const full_encoded = encodeBuffer(sequence, 48);
  return (0, import_core_util.uint8ArrayToString)(full_encoded, "base64");
}
function formatBase64Sequence(base64Sequence) {
  const lines = base64Sequence.match(/.{1,64}/g);
  let result = "";
  if (lines) {
    for (const line of lines) {
      result += line;
      result += "\n";
    }
  } else {
    throw new Error("Could not create correct PEM");
  }
  return result;
}
function convertJWKtoPEM(key) {
  let result = "";
  if (key.n && key.e) {
    const parts = [key.n, key.e];
    const encodedParts = parts.map((part) => encodeBuffer(part, 2));
    const base64Sequence = makeSequence(encodedParts);
    result += "-----BEGIN RSA PUBLIC KEY-----\n";
    result += formatBase64Sequence(base64Sequence);
    result += "-----END RSA PUBLIC KEY-----\n";
  }
  if (!result.length) {
    throw new Error("Unsupported key format for local operations");
  }
  return result.slice(0, -1);
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  convertJWKtoPEM
});
//# sourceMappingURL=conversions.js.map
