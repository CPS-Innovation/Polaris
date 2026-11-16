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
var parseKeyVaultIdentifier_exports = {};
__export(parseKeyVaultIdentifier_exports, {
  parseKeyVaultIdentifier: () => parseKeyVaultIdentifier
});
module.exports = __toCommonJS(parseKeyVaultIdentifier_exports);
function parseKeyVaultIdentifier(collection, identifier) {
  if (typeof collection !== "string" || !(collection = collection.trim())) {
    throw new Error("Invalid collection argument");
  }
  if (typeof identifier !== "string" || !(identifier = identifier.trim())) {
    throw new Error("Invalid identifier argument");
  }
  let baseUri;
  try {
    baseUri = new URL(identifier);
  } catch (e) {
    throw new Error(`Invalid ${collection} identifier: ${identifier}. Not a valid URI`);
  }
  const segments = (baseUri.pathname || "").split("/");
  if (segments.length !== 3 && segments.length !== 4) {
    throw new Error(
      `Invalid ${collection} identifier: ${identifier}. Bad number of segments: ${segments.length}`
    );
  }
  if (collection !== segments[1]) {
    throw new Error(
      `Invalid ${collection} identifier: ${identifier}. segment [1] should be "${collection}", found "${segments[1]}"`
    );
  }
  const vaultUrl = `${baseUri.protocol}//${baseUri.host}`;
  const name = segments[2];
  const version = segments.length === 4 ? segments[3] : void 0;
  return {
    vaultUrl,
    name,
    version
  };
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  parseKeyVaultIdentifier
});
//# sourceMappingURL=parseKeyVaultIdentifier.js.map
