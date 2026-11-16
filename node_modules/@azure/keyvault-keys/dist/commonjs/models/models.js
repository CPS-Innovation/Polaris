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
var models_exports = {};
__export(models_exports, {
  KnownDeletionRecoveryLevel: () => KnownDeletionRecoveryLevel,
  KnownJsonWebKeyCurveName: () => KnownJsonWebKeyCurveName,
  KnownJsonWebKeyEncryptionAlgorithm: () => KnownJsonWebKeyEncryptionAlgorithm,
  KnownJsonWebKeyOperation: () => KnownJsonWebKeyOperation,
  KnownJsonWebKeySignatureAlgorithm: () => KnownJsonWebKeySignatureAlgorithm,
  KnownJsonWebKeyType: () => KnownJsonWebKeyType,
  KnownKeyEncryptionAlgorithm: () => KnownKeyEncryptionAlgorithm,
  KnownVersions: () => KnownVersions,
  _deletedKeyListResultDeserializer: () => _deletedKeyListResultDeserializer,
  _keyListResultDeserializer: () => _keyListResultDeserializer,
  _keyVaultErrorErrorDeserializer: () => _keyVaultErrorErrorDeserializer,
  backupKeyResultDeserializer: () => backupKeyResultDeserializer,
  deletedKeyBundleDeserializer: () => deletedKeyBundleDeserializer,
  deletedKeyItemArrayDeserializer: () => deletedKeyItemArrayDeserializer,
  deletedKeyItemDeserializer: () => deletedKeyItemDeserializer,
  getRandomBytesRequestSerializer: () => getRandomBytesRequestSerializer,
  jsonWebKeyDeserializer: () => jsonWebKeyDeserializer,
  jsonWebKeySerializer: () => jsonWebKeySerializer,
  keyAttestationDeserializer: () => keyAttestationDeserializer,
  keyAttributesDeserializer: () => keyAttributesDeserializer,
  keyAttributesSerializer: () => keyAttributesSerializer,
  keyBundleDeserializer: () => keyBundleDeserializer,
  keyCreateParametersSerializer: () => keyCreateParametersSerializer,
  keyImportParametersSerializer: () => keyImportParametersSerializer,
  keyItemArrayDeserializer: () => keyItemArrayDeserializer,
  keyItemDeserializer: () => keyItemDeserializer,
  keyOperationResultDeserializer: () => keyOperationResultDeserializer,
  keyOperationsParametersSerializer: () => keyOperationsParametersSerializer,
  keyReleaseParametersSerializer: () => keyReleaseParametersSerializer,
  keyReleasePolicyDeserializer: () => keyReleasePolicyDeserializer,
  keyReleasePolicySerializer: () => keyReleasePolicySerializer,
  keyReleaseResultDeserializer: () => keyReleaseResultDeserializer,
  keyRestoreParametersSerializer: () => keyRestoreParametersSerializer,
  keyRotationPolicyAttributesDeserializer: () => keyRotationPolicyAttributesDeserializer,
  keyRotationPolicyAttributesSerializer: () => keyRotationPolicyAttributesSerializer,
  keyRotationPolicyDeserializer: () => keyRotationPolicyDeserializer,
  keyRotationPolicySerializer: () => keyRotationPolicySerializer,
  keySignParametersSerializer: () => keySignParametersSerializer,
  keyUpdateParametersSerializer: () => keyUpdateParametersSerializer,
  keyVaultErrorDeserializer: () => keyVaultErrorDeserializer,
  keyVerifyParametersSerializer: () => keyVerifyParametersSerializer,
  keyVerifyResultDeserializer: () => keyVerifyResultDeserializer,
  lifetimeActionsArrayDeserializer: () => lifetimeActionsArrayDeserializer,
  lifetimeActionsArraySerializer: () => lifetimeActionsArraySerializer,
  lifetimeActionsDeserializer: () => lifetimeActionsDeserializer,
  lifetimeActionsSerializer: () => lifetimeActionsSerializer,
  lifetimeActionsTriggerDeserializer: () => lifetimeActionsTriggerDeserializer,
  lifetimeActionsTriggerSerializer: () => lifetimeActionsTriggerSerializer,
  lifetimeActionsTypeDeserializer: () => lifetimeActionsTypeDeserializer,
  lifetimeActionsTypeSerializer: () => lifetimeActionsTypeSerializer,
  randomBytesDeserializer: () => randomBytesDeserializer
});
module.exports = __toCommonJS(models_exports);
var import_core_util = require("@azure/core-util");
function keyCreateParametersSerializer(item) {
  return {
    kty: item["kty"],
    key_size: item["keySize"],
    public_exponent: item["publicExponent"],
    key_ops: !item["keyOps"] ? item["keyOps"] : item["keyOps"].map((p) => {
      return p;
    }),
    attributes: !item["keyAttributes"] ? item["keyAttributes"] : keyAttributesSerializer(item["keyAttributes"]),
    tags: item["tags"],
    crv: item["curve"],
    release_policy: !item["releasePolicy"] ? item["releasePolicy"] : keyReleasePolicySerializer(item["releasePolicy"])
  };
}
var KnownJsonWebKeyType = /* @__PURE__ */ ((KnownJsonWebKeyType2) => {
  KnownJsonWebKeyType2["EC"] = "EC";
  KnownJsonWebKeyType2["ECHSM"] = "EC-HSM";
  KnownJsonWebKeyType2["RSA"] = "RSA";
  KnownJsonWebKeyType2["RSAHSM"] = "RSA-HSM";
  KnownJsonWebKeyType2["Oct"] = "oct";
  KnownJsonWebKeyType2["OctHSM"] = "oct-HSM";
  return KnownJsonWebKeyType2;
})(KnownJsonWebKeyType || {});
var KnownJsonWebKeyOperation = /* @__PURE__ */ ((KnownJsonWebKeyOperation2) => {
  KnownJsonWebKeyOperation2["Encrypt"] = "encrypt";
  KnownJsonWebKeyOperation2["Decrypt"] = "decrypt";
  KnownJsonWebKeyOperation2["Sign"] = "sign";
  KnownJsonWebKeyOperation2["Verify"] = "verify";
  KnownJsonWebKeyOperation2["WrapKey"] = "wrapKey";
  KnownJsonWebKeyOperation2["UnwrapKey"] = "unwrapKey";
  KnownJsonWebKeyOperation2["Import"] = "import";
  KnownJsonWebKeyOperation2["Export"] = "export";
  return KnownJsonWebKeyOperation2;
})(KnownJsonWebKeyOperation || {});
function keyAttributesSerializer(item) {
  return {
    enabled: item["enabled"],
    nbf: !item["notBefore"] ? item["notBefore"] : item["notBefore"].getTime() / 1e3 | 0,
    exp: !item["expires"] ? item["expires"] : item["expires"].getTime() / 1e3 | 0,
    exportable: item["exportable"]
  };
}
function keyAttributesDeserializer(item) {
  return {
    enabled: item["enabled"],
    notBefore: !item["nbf"] ? item["nbf"] : new Date(item["nbf"] * 1e3),
    expires: !item["exp"] ? item["exp"] : new Date(item["exp"] * 1e3),
    created: !item["created"] ? item["created"] : new Date(item["created"] * 1e3),
    updated: !item["updated"] ? item["updated"] : new Date(item["updated"] * 1e3),
    recoverableDays: item["recoverableDays"],
    recoveryLevel: item["recoveryLevel"],
    exportable: item["exportable"],
    hsmPlatform: item["hsmPlatform"],
    attestation: !item["attestation"] ? item["attestation"] : keyAttestationDeserializer(item["attestation"])
  };
}
var KnownDeletionRecoveryLevel = /* @__PURE__ */ ((KnownDeletionRecoveryLevel2) => {
  KnownDeletionRecoveryLevel2["Purgeable"] = "Purgeable";
  KnownDeletionRecoveryLevel2["RecoverablePurgeable"] = "Recoverable+Purgeable";
  KnownDeletionRecoveryLevel2["Recoverable"] = "Recoverable";
  KnownDeletionRecoveryLevel2["RecoverableProtectedSubscription"] = "Recoverable+ProtectedSubscription";
  KnownDeletionRecoveryLevel2["CustomizedRecoverablePurgeable"] = "CustomizedRecoverable+Purgeable";
  KnownDeletionRecoveryLevel2["CustomizedRecoverable"] = "CustomizedRecoverable";
  KnownDeletionRecoveryLevel2["CustomizedRecoverableProtectedSubscription"] = "CustomizedRecoverable+ProtectedSubscription";
  return KnownDeletionRecoveryLevel2;
})(KnownDeletionRecoveryLevel || {});
function keyAttestationDeserializer(item) {
  return {
    certificatePemFile: !item["certificatePemFile"] ? item["certificatePemFile"] : typeof item["certificatePemFile"] === "string" ? (0, import_core_util.stringToUint8Array)(item["certificatePemFile"], "base64url") : item["certificatePemFile"],
    privateKeyAttestation: !item["privateKeyAttestation"] ? item["privateKeyAttestation"] : typeof item["privateKeyAttestation"] === "string" ? (0, import_core_util.stringToUint8Array)(item["privateKeyAttestation"], "base64url") : item["privateKeyAttestation"],
    publicKeyAttestation: !item["publicKeyAttestation"] ? item["publicKeyAttestation"] : typeof item["publicKeyAttestation"] === "string" ? (0, import_core_util.stringToUint8Array)(item["publicKeyAttestation"], "base64url") : item["publicKeyAttestation"],
    version: item["version"]
  };
}
var KnownJsonWebKeyCurveName = /* @__PURE__ */ ((KnownJsonWebKeyCurveName2) => {
  KnownJsonWebKeyCurveName2["P256"] = "P-256";
  KnownJsonWebKeyCurveName2["P384"] = "P-384";
  KnownJsonWebKeyCurveName2["P521"] = "P-521";
  KnownJsonWebKeyCurveName2["P256K"] = "P-256K";
  return KnownJsonWebKeyCurveName2;
})(KnownJsonWebKeyCurveName || {});
function keyReleasePolicySerializer(item) {
  return {
    contentType: item["contentType"],
    immutable: item["immutable"],
    data: !item["encodedPolicy"] ? item["encodedPolicy"] : (0, import_core_util.uint8ArrayToString)(item["encodedPolicy"], "base64url")
  };
}
function keyReleasePolicyDeserializer(item) {
  return {
    contentType: item["contentType"],
    immutable: item["immutable"],
    encodedPolicy: !item["data"] ? item["data"] : typeof item["data"] === "string" ? (0, import_core_util.stringToUint8Array)(item["data"], "base64url") : item["data"]
  };
}
function keyBundleDeserializer(item) {
  return {
    key: !item["key"] ? item["key"] : jsonWebKeyDeserializer(item["key"]),
    attributes: !item["attributes"] ? item["attributes"] : keyAttributesDeserializer(item["attributes"]),
    tags: item["tags"],
    managed: item["managed"],
    releasePolicy: !item["release_policy"] ? item["release_policy"] : keyReleasePolicyDeserializer(item["release_policy"])
  };
}
function jsonWebKeySerializer(item) {
  return {
    kid: item["kid"],
    kty: item["kty"],
    key_ops: !item["keyOps"] ? item["keyOps"] : item["keyOps"].map((p) => {
      return p;
    }),
    n: !item["n"] ? item["n"] : (0, import_core_util.uint8ArrayToString)(item["n"], "base64url"),
    e: !item["e"] ? item["e"] : (0, import_core_util.uint8ArrayToString)(item["e"], "base64url"),
    d: !item["d"] ? item["d"] : (0, import_core_util.uint8ArrayToString)(item["d"], "base64url"),
    dp: !item["dp"] ? item["dp"] : (0, import_core_util.uint8ArrayToString)(item["dp"], "base64url"),
    dq: !item["dq"] ? item["dq"] : (0, import_core_util.uint8ArrayToString)(item["dq"], "base64url"),
    qi: !item["qi"] ? item["qi"] : (0, import_core_util.uint8ArrayToString)(item["qi"], "base64url"),
    p: !item["p"] ? item["p"] : (0, import_core_util.uint8ArrayToString)(item["p"], "base64url"),
    q: !item["q"] ? item["q"] : (0, import_core_util.uint8ArrayToString)(item["q"], "base64url"),
    k: !item["k"] ? item["k"] : (0, import_core_util.uint8ArrayToString)(item["k"], "base64url"),
    key_hsm: !item["t"] ? item["t"] : (0, import_core_util.uint8ArrayToString)(item["t"], "base64url"),
    crv: item["crv"],
    x: !item["x"] ? item["x"] : (0, import_core_util.uint8ArrayToString)(item["x"], "base64url"),
    y: !item["y"] ? item["y"] : (0, import_core_util.uint8ArrayToString)(item["y"], "base64url")
  };
}
function jsonWebKeyDeserializer(item) {
  return {
    kid: item["kid"],
    kty: item["kty"],
    keyOps: !item["key_ops"] ? item["key_ops"] : item["key_ops"].map((p) => {
      return p;
    }),
    n: !item["n"] ? item["n"] : typeof item["n"] === "string" ? (0, import_core_util.stringToUint8Array)(item["n"], "base64url") : item["n"],
    e: !item["e"] ? item["e"] : typeof item["e"] === "string" ? (0, import_core_util.stringToUint8Array)(item["e"], "base64url") : item["e"],
    d: !item["d"] ? item["d"] : typeof item["d"] === "string" ? (0, import_core_util.stringToUint8Array)(item["d"], "base64url") : item["d"],
    dp: !item["dp"] ? item["dp"] : typeof item["dp"] === "string" ? (0, import_core_util.stringToUint8Array)(item["dp"], "base64url") : item["dp"],
    dq: !item["dq"] ? item["dq"] : typeof item["dq"] === "string" ? (0, import_core_util.stringToUint8Array)(item["dq"], "base64url") : item["dq"],
    qi: !item["qi"] ? item["qi"] : typeof item["qi"] === "string" ? (0, import_core_util.stringToUint8Array)(item["qi"], "base64url") : item["qi"],
    p: !item["p"] ? item["p"] : typeof item["p"] === "string" ? (0, import_core_util.stringToUint8Array)(item["p"], "base64url") : item["p"],
    q: !item["q"] ? item["q"] : typeof item["q"] === "string" ? (0, import_core_util.stringToUint8Array)(item["q"], "base64url") : item["q"],
    k: !item["k"] ? item["k"] : typeof item["k"] === "string" ? (0, import_core_util.stringToUint8Array)(item["k"], "base64url") : item["k"],
    t: !item["key_hsm"] ? item["key_hsm"] : typeof item["key_hsm"] === "string" ? (0, import_core_util.stringToUint8Array)(item["key_hsm"], "base64url") : item["key_hsm"],
    crv: item["crv"],
    x: !item["x"] ? item["x"] : typeof item["x"] === "string" ? (0, import_core_util.stringToUint8Array)(item["x"], "base64url") : item["x"],
    y: !item["y"] ? item["y"] : typeof item["y"] === "string" ? (0, import_core_util.stringToUint8Array)(item["y"], "base64url") : item["y"]
  };
}
function keyVaultErrorDeserializer(item) {
  return {
    error: !item["error"] ? item["error"] : _keyVaultErrorErrorDeserializer(item["error"])
  };
}
function _keyVaultErrorErrorDeserializer(item) {
  return {
    code: item["code"],
    message: item["message"],
    innerError: !item["innererror"] ? item["innererror"] : _keyVaultErrorErrorDeserializer(item["innererror"])
  };
}
function keyImportParametersSerializer(item) {
  return {
    Hsm: item["hsm"],
    key: jsonWebKeySerializer(item["key"]),
    attributes: !item["keyAttributes"] ? item["keyAttributes"] : keyAttributesSerializer(item["keyAttributes"]),
    tags: item["tags"],
    release_policy: !item["releasePolicy"] ? item["releasePolicy"] : keyReleasePolicySerializer(item["releasePolicy"])
  };
}
function deletedKeyBundleDeserializer(item) {
  return {
    key: !item["key"] ? item["key"] : jsonWebKeyDeserializer(item["key"]),
    attributes: !item["attributes"] ? item["attributes"] : keyAttributesDeserializer(item["attributes"]),
    tags: item["tags"],
    managed: item["managed"],
    releasePolicy: !item["release_policy"] ? item["release_policy"] : keyReleasePolicyDeserializer(item["release_policy"]),
    recoveryId: item["recoveryId"],
    scheduledPurgeDate: !item["scheduledPurgeDate"] ? item["scheduledPurgeDate"] : new Date(item["scheduledPurgeDate"] * 1e3),
    deletedDate: !item["deletedDate"] ? item["deletedDate"] : new Date(item["deletedDate"] * 1e3)
  };
}
function keyUpdateParametersSerializer(item) {
  return {
    key_ops: !item["keyOps"] ? item["keyOps"] : item["keyOps"].map((p) => {
      return p;
    }),
    attributes: !item["keyAttributes"] ? item["keyAttributes"] : keyAttributesSerializer(item["keyAttributes"]),
    tags: item["tags"],
    release_policy: !item["releasePolicy"] ? item["releasePolicy"] : keyReleasePolicySerializer(item["releasePolicy"])
  };
}
function _keyListResultDeserializer(item) {
  return {
    value: !item["value"] ? item["value"] : keyItemArrayDeserializer(item["value"]),
    nextLink: item["nextLink"]
  };
}
function keyItemArrayDeserializer(result) {
  return result.map((item) => {
    return keyItemDeserializer(item);
  });
}
function keyItemDeserializer(item) {
  return {
    kid: item["kid"],
    attributes: !item["attributes"] ? item["attributes"] : keyAttributesDeserializer(item["attributes"]),
    tags: item["tags"],
    managed: item["managed"]
  };
}
function backupKeyResultDeserializer(item) {
  return {
    value: !item["value"] ? item["value"] : typeof item["value"] === "string" ? (0, import_core_util.stringToUint8Array)(item["value"], "base64url") : item["value"]
  };
}
function keyRestoreParametersSerializer(item) {
  return { value: (0, import_core_util.uint8ArrayToString)(item["keyBundleBackup"], "base64url") };
}
function keyOperationsParametersSerializer(item) {
  return {
    alg: item["algorithm"],
    value: (0, import_core_util.uint8ArrayToString)(item["value"], "base64url"),
    iv: !item["iv"] ? item["iv"] : (0, import_core_util.uint8ArrayToString)(item["iv"], "base64url"),
    aad: !item["aad"] ? item["aad"] : (0, import_core_util.uint8ArrayToString)(item["aad"], "base64url"),
    tag: !item["tag"] ? item["tag"] : (0, import_core_util.uint8ArrayToString)(item["tag"], "base64url")
  };
}
var KnownJsonWebKeyEncryptionAlgorithm = /* @__PURE__ */ ((KnownJsonWebKeyEncryptionAlgorithm2) => {
  KnownJsonWebKeyEncryptionAlgorithm2["RSAOaep"] = "RSA-OAEP";
  KnownJsonWebKeyEncryptionAlgorithm2["RSAOaep256"] = "RSA-OAEP-256";
  KnownJsonWebKeyEncryptionAlgorithm2["RSA15"] = "RSA1_5";
  KnownJsonWebKeyEncryptionAlgorithm2["A128GCM"] = "A128GCM";
  KnownJsonWebKeyEncryptionAlgorithm2["A192GCM"] = "A192GCM";
  KnownJsonWebKeyEncryptionAlgorithm2["A256GCM"] = "A256GCM";
  KnownJsonWebKeyEncryptionAlgorithm2["A128KW"] = "A128KW";
  KnownJsonWebKeyEncryptionAlgorithm2["A192KW"] = "A192KW";
  KnownJsonWebKeyEncryptionAlgorithm2["A256KW"] = "A256KW";
  KnownJsonWebKeyEncryptionAlgorithm2["A128CBC"] = "A128CBC";
  KnownJsonWebKeyEncryptionAlgorithm2["A192CBC"] = "A192CBC";
  KnownJsonWebKeyEncryptionAlgorithm2["A256CBC"] = "A256CBC";
  KnownJsonWebKeyEncryptionAlgorithm2["A128Cbcpad"] = "A128CBCPAD";
  KnownJsonWebKeyEncryptionAlgorithm2["A192Cbcpad"] = "A192CBCPAD";
  KnownJsonWebKeyEncryptionAlgorithm2["A256Cbcpad"] = "A256CBCPAD";
  KnownJsonWebKeyEncryptionAlgorithm2["CkmAesKeyWrap"] = "CKM_AES_KEY_WRAP";
  KnownJsonWebKeyEncryptionAlgorithm2["CkmAesKeyWrapPad"] = "CKM_AES_KEY_WRAP_PAD";
  return KnownJsonWebKeyEncryptionAlgorithm2;
})(KnownJsonWebKeyEncryptionAlgorithm || {});
function keyOperationResultDeserializer(item) {
  return {
    kid: item["kid"],
    result: !item["value"] ? item["value"] : typeof item["value"] === "string" ? (0, import_core_util.stringToUint8Array)(item["value"], "base64url") : item["value"],
    iv: !item["iv"] ? item["iv"] : typeof item["iv"] === "string" ? (0, import_core_util.stringToUint8Array)(item["iv"], "base64url") : item["iv"],
    authenticationTag: !item["tag"] ? item["tag"] : typeof item["tag"] === "string" ? (0, import_core_util.stringToUint8Array)(item["tag"], "base64url") : item["tag"],
    additionalAuthenticatedData: !item["aad"] ? item["aad"] : typeof item["aad"] === "string" ? (0, import_core_util.stringToUint8Array)(item["aad"], "base64url") : item["aad"]
  };
}
function keySignParametersSerializer(item) {
  return {
    alg: item["algorithm"],
    value: (0, import_core_util.uint8ArrayToString)(item["value"], "base64url")
  };
}
var KnownJsonWebKeySignatureAlgorithm = /* @__PURE__ */ ((KnownJsonWebKeySignatureAlgorithm2) => {
  KnownJsonWebKeySignatureAlgorithm2["PS256"] = "PS256";
  KnownJsonWebKeySignatureAlgorithm2["PS384"] = "PS384";
  KnownJsonWebKeySignatureAlgorithm2["PS512"] = "PS512";
  KnownJsonWebKeySignatureAlgorithm2["RS256"] = "RS256";
  KnownJsonWebKeySignatureAlgorithm2["RS384"] = "RS384";
  KnownJsonWebKeySignatureAlgorithm2["RS512"] = "RS512";
  KnownJsonWebKeySignatureAlgorithm2["HS256"] = "HS256";
  KnownJsonWebKeySignatureAlgorithm2["HS384"] = "HS384";
  KnownJsonWebKeySignatureAlgorithm2["HS512"] = "HS512";
  KnownJsonWebKeySignatureAlgorithm2["Rsnull"] = "RSNULL";
  KnownJsonWebKeySignatureAlgorithm2["ES256"] = "ES256";
  KnownJsonWebKeySignatureAlgorithm2["ES384"] = "ES384";
  KnownJsonWebKeySignatureAlgorithm2["ES512"] = "ES512";
  KnownJsonWebKeySignatureAlgorithm2["ES256K"] = "ES256K";
  return KnownJsonWebKeySignatureAlgorithm2;
})(KnownJsonWebKeySignatureAlgorithm || {});
function keyVerifyParametersSerializer(item) {
  return {
    alg: item["algorithm"],
    digest: (0, import_core_util.uint8ArrayToString)(item["digest"], "base64url"),
    value: (0, import_core_util.uint8ArrayToString)(item["signature"], "base64url")
  };
}
function keyVerifyResultDeserializer(item) {
  return {
    value: item["value"]
  };
}
function keyReleaseParametersSerializer(item) {
  return {
    target: item["targetAttestationToken"],
    nonce: item["nonce"],
    enc: item["enc"]
  };
}
var KnownKeyEncryptionAlgorithm = /* @__PURE__ */ ((KnownKeyEncryptionAlgorithm2) => {
  KnownKeyEncryptionAlgorithm2["CkmRsaAesKeyWrap"] = "CKM_RSA_AES_KEY_WRAP";
  KnownKeyEncryptionAlgorithm2["RsaAesKeyWrap256"] = "RSA_AES_KEY_WRAP_256";
  KnownKeyEncryptionAlgorithm2["RsaAesKeyWrap384"] = "RSA_AES_KEY_WRAP_384";
  return KnownKeyEncryptionAlgorithm2;
})(KnownKeyEncryptionAlgorithm || {});
function keyReleaseResultDeserializer(item) {
  return {
    value: item["value"]
  };
}
function _deletedKeyListResultDeserializer(item) {
  return {
    value: !item["value"] ? item["value"] : deletedKeyItemArrayDeserializer(item["value"]),
    nextLink: item["nextLink"]
  };
}
function deletedKeyItemArrayDeserializer(result) {
  return result.map((item) => {
    return deletedKeyItemDeserializer(item);
  });
}
function deletedKeyItemDeserializer(item) {
  return {
    kid: item["kid"],
    attributes: !item["attributes"] ? item["attributes"] : keyAttributesDeserializer(item["attributes"]),
    tags: item["tags"],
    managed: item["managed"],
    recoveryId: item["recoveryId"],
    scheduledPurgeDate: !item["scheduledPurgeDate"] ? item["scheduledPurgeDate"] : new Date(item["scheduledPurgeDate"] * 1e3),
    deletedDate: !item["deletedDate"] ? item["deletedDate"] : new Date(item["deletedDate"] * 1e3)
  };
}
function keyRotationPolicySerializer(item) {
  return {
    lifetimeActions: !item["lifetimeActions"] ? item["lifetimeActions"] : lifetimeActionsArraySerializer(item["lifetimeActions"]),
    attributes: !item["attributes"] ? item["attributes"] : keyRotationPolicyAttributesSerializer(item["attributes"])
  };
}
function keyRotationPolicyDeserializer(item) {
  return {
    id: item["id"],
    lifetimeActions: !item["lifetimeActions"] ? item["lifetimeActions"] : lifetimeActionsArrayDeserializer(item["lifetimeActions"]),
    attributes: !item["attributes"] ? item["attributes"] : keyRotationPolicyAttributesDeserializer(item["attributes"])
  };
}
function lifetimeActionsArraySerializer(result) {
  return result.map((item) => {
    return lifetimeActionsSerializer(item);
  });
}
function lifetimeActionsArrayDeserializer(result) {
  return result.map((item) => {
    return lifetimeActionsDeserializer(item);
  });
}
function lifetimeActionsSerializer(item) {
  return {
    trigger: !item["trigger"] ? item["trigger"] : lifetimeActionsTriggerSerializer(item["trigger"]),
    action: !item["action"] ? item["action"] : lifetimeActionsTypeSerializer(item["action"])
  };
}
function lifetimeActionsDeserializer(item) {
  return {
    trigger: !item["trigger"] ? item["trigger"] : lifetimeActionsTriggerDeserializer(item["trigger"]),
    action: !item["action"] ? item["action"] : lifetimeActionsTypeDeserializer(item["action"])
  };
}
function lifetimeActionsTriggerSerializer(item) {
  return {
    timeAfterCreate: item["timeAfterCreate"],
    timeBeforeExpiry: item["timeBeforeExpiry"]
  };
}
function lifetimeActionsTriggerDeserializer(item) {
  return {
    timeAfterCreate: item["timeAfterCreate"],
    timeBeforeExpiry: item["timeBeforeExpiry"]
  };
}
function lifetimeActionsTypeSerializer(item) {
  return { type: item["type"] };
}
function lifetimeActionsTypeDeserializer(item) {
  return {
    type: item["type"]
  };
}
function keyRotationPolicyAttributesSerializer(item) {
  return { expiryTime: item["expiryTime"] };
}
function keyRotationPolicyAttributesDeserializer(item) {
  return {
    expiryTime: item["expiryTime"],
    created: !item["created"] ? item["created"] : new Date(item["created"] * 1e3),
    updated: !item["updated"] ? item["updated"] : new Date(item["updated"] * 1e3)
  };
}
function getRandomBytesRequestSerializer(item) {
  return { count: item["count"] };
}
function randomBytesDeserializer(item) {
  return {
    value: typeof item["value"] === "string" ? (0, import_core_util.stringToUint8Array)(item["value"], "base64url") : item["value"]
  };
}
var KnownVersions = /* @__PURE__ */ ((KnownVersions2) => {
  KnownVersions2["V75"] = "7.5";
  KnownVersions2["V76Preview2"] = "7.6-preview.2";
  KnownVersions2["V76"] = "7.6";
  KnownVersions2["V20250701"] = "2025-07-01";
  return KnownVersions2;
})(KnownVersions || {});
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  KnownDeletionRecoveryLevel,
  KnownJsonWebKeyCurveName,
  KnownJsonWebKeyEncryptionAlgorithm,
  KnownJsonWebKeyOperation,
  KnownJsonWebKeySignatureAlgorithm,
  KnownJsonWebKeyType,
  KnownKeyEncryptionAlgorithm,
  KnownVersions,
  _deletedKeyListResultDeserializer,
  _keyListResultDeserializer,
  _keyVaultErrorErrorDeserializer,
  backupKeyResultDeserializer,
  deletedKeyBundleDeserializer,
  deletedKeyItemArrayDeserializer,
  deletedKeyItemDeserializer,
  getRandomBytesRequestSerializer,
  jsonWebKeyDeserializer,
  jsonWebKeySerializer,
  keyAttestationDeserializer,
  keyAttributesDeserializer,
  keyAttributesSerializer,
  keyBundleDeserializer,
  keyCreateParametersSerializer,
  keyImportParametersSerializer,
  keyItemArrayDeserializer,
  keyItemDeserializer,
  keyOperationResultDeserializer,
  keyOperationsParametersSerializer,
  keyReleaseParametersSerializer,
  keyReleasePolicyDeserializer,
  keyReleasePolicySerializer,
  keyReleaseResultDeserializer,
  keyRestoreParametersSerializer,
  keyRotationPolicyAttributesDeserializer,
  keyRotationPolicyAttributesSerializer,
  keyRotationPolicyDeserializer,
  keyRotationPolicySerializer,
  keySignParametersSerializer,
  keyUpdateParametersSerializer,
  keyVaultErrorDeserializer,
  keyVerifyParametersSerializer,
  keyVerifyResultDeserializer,
  lifetimeActionsArrayDeserializer,
  lifetimeActionsArraySerializer,
  lifetimeActionsDeserializer,
  lifetimeActionsSerializer,
  lifetimeActionsTriggerDeserializer,
  lifetimeActionsTriggerSerializer,
  lifetimeActionsTypeDeserializer,
  lifetimeActionsTypeSerializer,
  randomBytesDeserializer
});
//# sourceMappingURL=models.js.map
