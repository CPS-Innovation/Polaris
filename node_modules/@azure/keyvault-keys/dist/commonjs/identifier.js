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
var identifier_exports = {};
__export(identifier_exports, {
  parseKeyVaultKeyIdentifier: () => parseKeyVaultKeyIdentifier
});
module.exports = __toCommonJS(identifier_exports);
var import_keyvault_common = require("@azure/keyvault-common");
function parseKeyVaultKeyIdentifier(id) {
  const urlParts = id.split("/");
  const collection = urlParts[3];
  return {
    sourceId: id,
    ...(0, import_keyvault_common.parseKeyVaultIdentifier)(collection, id)
  };
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  parseKeyVaultKeyIdentifier
});
//# sourceMappingURL=identifier.js.map
