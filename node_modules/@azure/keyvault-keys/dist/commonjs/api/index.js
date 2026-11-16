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
var api_exports = {};
__export(api_exports, {
  backupKey: () => import_operations.backupKey,
  createKey: () => import_operations.createKey,
  createKeyVault: () => import_keyVaultContext.createKeyVault,
  decrypt: () => import_operations.decrypt,
  deleteKey: () => import_operations.deleteKey,
  encrypt: () => import_operations.encrypt,
  getDeletedKey: () => import_operations.getDeletedKey,
  getDeletedKeys: () => import_operations.getDeletedKeys,
  getKey: () => import_operations.getKey,
  getKeyAttestation: () => import_operations.getKeyAttestation,
  getKeyRotationPolicy: () => import_operations.getKeyRotationPolicy,
  getKeyVersions: () => import_operations.getKeyVersions,
  getKeys: () => import_operations.getKeys,
  getRandomBytes: () => import_operations.getRandomBytes,
  importKey: () => import_operations.importKey,
  purgeDeletedKey: () => import_operations.purgeDeletedKey,
  recoverDeletedKey: () => import_operations.recoverDeletedKey,
  release: () => import_operations.release,
  restoreKey: () => import_operations.restoreKey,
  rotateKey: () => import_operations.rotateKey,
  sign: () => import_operations.sign,
  unwrapKey: () => import_operations.unwrapKey,
  updateKey: () => import_operations.updateKey,
  updateKeyRotationPolicy: () => import_operations.updateKeyRotationPolicy,
  verify: () => import_operations.verify,
  wrapKey: () => import_operations.wrapKey
});
module.exports = __toCommonJS(api_exports);
var import_keyVaultContext = require("./keyVaultContext.js");
var import_operations = require("./operations.js");
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  backupKey,
  createKey,
  createKeyVault,
  decrypt,
  deleteKey,
  encrypt,
  getDeletedKey,
  getDeletedKeys,
  getKey,
  getKeyAttestation,
  getKeyRotationPolicy,
  getKeyVersions,
  getKeys,
  getRandomBytes,
  importKey,
  purgeDeletedKey,
  recoverDeletedKey,
  release,
  restoreKey,
  rotateKey,
  sign,
  unwrapKey,
  updateKey,
  updateKeyRotationPolicy,
  verify,
  wrapKey
});
//# sourceMappingURL=index.js.map
