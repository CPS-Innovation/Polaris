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
var cryptographyClientModels_exports = {};
__export(cryptographyClientModels_exports, {
  KnownEncryptionAlgorithms: () => import_models.KnownJsonWebKeyEncryptionAlgorithm,
  KnownKeyCurveNames: () => import_models.KnownJsonWebKeyCurveName,
  KnownKeyExportEncryptionAlgorithm: () => import_models.KnownKeyEncryptionAlgorithm,
  KnownKeyTypes: () => import_models.KnownJsonWebKeyType,
  KnownSignatureAlgorithms: () => import_models.KnownJsonWebKeySignatureAlgorithm
});
module.exports = __toCommonJS(cryptographyClientModels_exports);
var import_models = require("./models/models.js");
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  KnownEncryptionAlgorithms,
  KnownKeyCurveNames,
  KnownKeyExportEncryptionAlgorithm,
  KnownKeyTypes,
  KnownSignatureAlgorithms
});
//# sourceMappingURL=cryptographyClientModels.js.map
