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
var keysModels_exports = {};
__export(keysModels_exports, {
  KnownKeyOperations: () => KnownKeyOperations,
  LATEST_API_VERSION: () => LATEST_API_VERSION
});
module.exports = __toCommonJS(keysModels_exports);
const LATEST_API_VERSION = "2025-07-01";
var KnownKeyOperations = /* @__PURE__ */ ((KnownKeyOperations2) => {
  KnownKeyOperations2["Encrypt"] = "encrypt";
  KnownKeyOperations2["Decrypt"] = "decrypt";
  KnownKeyOperations2["Sign"] = "sign";
  KnownKeyOperations2["Verify"] = "verify";
  KnownKeyOperations2["WrapKey"] = "wrapKey";
  KnownKeyOperations2["UnwrapKey"] = "unwrapKey";
  KnownKeyOperations2["Import"] = "import";
  return KnownKeyOperations2;
})(KnownKeyOperations || {});
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  KnownKeyOperations,
  LATEST_API_VERSION
});
//# sourceMappingURL=keysModels.js.map
