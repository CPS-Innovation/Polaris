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
var operations_exports = {};
__export(operations_exports, {
  _backupKeyDeserialize: () => _backupKeyDeserialize,
  _backupKeySend: () => _backupKeySend,
  _createKeyDeserialize: () => _createKeyDeserialize,
  _createKeySend: () => _createKeySend,
  _decryptDeserialize: () => _decryptDeserialize,
  _decryptSend: () => _decryptSend,
  _deleteKeyDeserialize: () => _deleteKeyDeserialize,
  _deleteKeySend: () => _deleteKeySend,
  _encryptDeserialize: () => _encryptDeserialize,
  _encryptSend: () => _encryptSend,
  _getDeletedKeyDeserialize: () => _getDeletedKeyDeserialize,
  _getDeletedKeySend: () => _getDeletedKeySend,
  _getDeletedKeysDeserialize: () => _getDeletedKeysDeserialize,
  _getDeletedKeysSend: () => _getDeletedKeysSend,
  _getKeyAttestationDeserialize: () => _getKeyAttestationDeserialize,
  _getKeyAttestationSend: () => _getKeyAttestationSend,
  _getKeyDeserialize: () => _getKeyDeserialize,
  _getKeyRotationPolicyDeserialize: () => _getKeyRotationPolicyDeserialize,
  _getKeyRotationPolicySend: () => _getKeyRotationPolicySend,
  _getKeySend: () => _getKeySend,
  _getKeyVersionsDeserialize: () => _getKeyVersionsDeserialize,
  _getKeyVersionsSend: () => _getKeyVersionsSend,
  _getKeysDeserialize: () => _getKeysDeserialize,
  _getKeysSend: () => _getKeysSend,
  _getRandomBytesDeserialize: () => _getRandomBytesDeserialize,
  _getRandomBytesSend: () => _getRandomBytesSend,
  _importKeyDeserialize: () => _importKeyDeserialize,
  _importKeySend: () => _importKeySend,
  _purgeDeletedKeyDeserialize: () => _purgeDeletedKeyDeserialize,
  _purgeDeletedKeySend: () => _purgeDeletedKeySend,
  _recoverDeletedKeyDeserialize: () => _recoverDeletedKeyDeserialize,
  _recoverDeletedKeySend: () => _recoverDeletedKeySend,
  _releaseDeserialize: () => _releaseDeserialize,
  _releaseSend: () => _releaseSend,
  _restoreKeyDeserialize: () => _restoreKeyDeserialize,
  _restoreKeySend: () => _restoreKeySend,
  _rotateKeyDeserialize: () => _rotateKeyDeserialize,
  _rotateKeySend: () => _rotateKeySend,
  _signDeserialize: () => _signDeserialize,
  _signSend: () => _signSend,
  _unwrapKeyDeserialize: () => _unwrapKeyDeserialize,
  _unwrapKeySend: () => _unwrapKeySend,
  _updateKeyDeserialize: () => _updateKeyDeserialize,
  _updateKeyRotationPolicyDeserialize: () => _updateKeyRotationPolicyDeserialize,
  _updateKeyRotationPolicySend: () => _updateKeyRotationPolicySend,
  _updateKeySend: () => _updateKeySend,
  _verifyDeserialize: () => _verifyDeserialize,
  _verifySend: () => _verifySend,
  _wrapKeyDeserialize: () => _wrapKeyDeserialize,
  _wrapKeySend: () => _wrapKeySend,
  backupKey: () => backupKey,
  createKey: () => createKey,
  decrypt: () => decrypt,
  deleteKey: () => deleteKey,
  encrypt: () => encrypt,
  getDeletedKey: () => getDeletedKey,
  getDeletedKeys: () => getDeletedKeys,
  getKey: () => getKey,
  getKeyAttestation: () => getKeyAttestation,
  getKeyRotationPolicy: () => getKeyRotationPolicy,
  getKeyVersions: () => getKeyVersions,
  getKeys: () => getKeys,
  getRandomBytes: () => getRandomBytes,
  importKey: () => importKey,
  purgeDeletedKey: () => purgeDeletedKey,
  recoverDeletedKey: () => recoverDeletedKey,
  release: () => release,
  restoreKey: () => restoreKey,
  rotateKey: () => rotateKey,
  sign: () => sign,
  unwrapKey: () => unwrapKey,
  updateKey: () => updateKey,
  updateKeyRotationPolicy: () => updateKeyRotationPolicy,
  verify: () => verify,
  wrapKey: () => wrapKey
});
module.exports = __toCommonJS(operations_exports);
var import_models = require("../models/models.js");
var import_pagingHelpers = require("../static-helpers/pagingHelpers.js");
var import_urlTemplate = require("../static-helpers/urlTemplate.js");
var import_core_client = require("@azure-rest/core-client");
function _getKeyAttestationSend(context, keyName, keyVersion, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}{/key-version}/attestation{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getKeyAttestationDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function getKeyAttestation(context, keyName, keyVersion, options = { requestOptions: {} }) {
  const result = await _getKeyAttestationSend(context, keyName, keyVersion, options);
  return _getKeyAttestationDeserialize(result);
}
function _getRandomBytesSend(context, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/rng{?api%2Dversion}",
    {
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.getRandomBytesRequestSerializer)(parameters)
  });
}
async function _getRandomBytesDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.randomBytesDeserializer)(result.body);
}
async function getRandomBytes(context, parameters, options = { requestOptions: {} }) {
  const result = await _getRandomBytesSend(context, parameters, options);
  return _getRandomBytesDeserialize(result);
}
function _updateKeyRotationPolicySend(context, keyName, keyRotationPolicy, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/rotationpolicy{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).put({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyRotationPolicySerializer)(keyRotationPolicy)
  });
}
async function _updateKeyRotationPolicyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyRotationPolicyDeserializer)(result.body);
}
async function updateKeyRotationPolicy(context, keyName, keyRotationPolicy, options = { requestOptions: {} }) {
  const result = await _updateKeyRotationPolicySend(context, keyName, keyRotationPolicy, options);
  return _updateKeyRotationPolicyDeserialize(result);
}
function _getKeyRotationPolicySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/rotationpolicy{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getKeyRotationPolicyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyRotationPolicyDeserializer)(result.body);
}
async function getKeyRotationPolicy(context, keyName, options = { requestOptions: {} }) {
  const result = await _getKeyRotationPolicySend(context, keyName, options);
  return _getKeyRotationPolicyDeserialize(result);
}
function _recoverDeletedKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/deletedkeys/{key-name}/recover{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _recoverDeletedKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function recoverDeletedKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _recoverDeletedKeySend(context, keyName, options);
  return _recoverDeletedKeyDeserialize(result);
}
function _purgeDeletedKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/deletedkeys/{key-name}{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).delete({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _purgeDeletedKeyDeserialize(result) {
  const expectedStatuses = ["204"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return;
}
async function purgeDeletedKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _purgeDeletedKeySend(context, keyName, options);
  return _purgeDeletedKeyDeserialize(result);
}
function _getDeletedKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/deletedkeys/{key-name}{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getDeletedKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.deletedKeyBundleDeserializer)(result.body);
}
async function getDeletedKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _getDeletedKeySend(context, keyName, options);
  return _getDeletedKeyDeserialize(result);
}
function _getDeletedKeysSend(context, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/deletedkeys{?api%2Dversion,maxresults}",
    {
      "api%2Dversion": context.apiVersion,
      maxresults: options?.maxresults
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getDeletedKeysDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models._deletedKeyListResultDeserializer)(result.body);
}
function getDeletedKeys(context, options = { requestOptions: {} }) {
  return (0, import_pagingHelpers.buildPagedAsyncIterator)(
    context,
    () => _getDeletedKeysSend(context, options),
    _getDeletedKeysDeserialize,
    ["200"],
    { itemName: "value", nextLinkName: "nextLink" }
  );
}
function _releaseSend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}{/key-version}/release{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyReleaseParametersSerializer)(parameters)
  });
}
async function _releaseDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyReleaseResultDeserializer)(result.body);
}
async function release(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _releaseSend(context, keyName, keyVersion, parameters, options);
  return _releaseDeserialize(result);
}
function _unwrapKeySend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/unwrapkey{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyOperationsParametersSerializer)(parameters)
  });
}
async function _unwrapKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyOperationResultDeserializer)(result.body);
}
async function unwrapKey(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _unwrapKeySend(context, keyName, keyVersion, parameters, options);
  return _unwrapKeyDeserialize(result);
}
function _wrapKeySend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/wrapkey{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyOperationsParametersSerializer)(parameters)
  });
}
async function _wrapKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyOperationResultDeserializer)(result.body);
}
async function wrapKey(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _wrapKeySend(context, keyName, keyVersion, parameters, options);
  return _wrapKeyDeserialize(result);
}
function _verifySend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/verify{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyVerifyParametersSerializer)(parameters)
  });
}
async function _verifyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyVerifyResultDeserializer)(result.body);
}
async function verify(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _verifySend(context, keyName, keyVersion, parameters, options);
  return _verifyDeserialize(result);
}
function _signSend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/sign{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keySignParametersSerializer)(parameters)
  });
}
async function _signDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyOperationResultDeserializer)(result.body);
}
async function sign(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _signSend(context, keyName, keyVersion, parameters, options);
  return _signDeserialize(result);
}
function _decryptSend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/decrypt{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyOperationsParametersSerializer)(parameters)
  });
}
async function _decryptDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyOperationResultDeserializer)(result.body);
}
async function decrypt(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _decryptSend(context, keyName, keyVersion, parameters, options);
  return _decryptDeserialize(result);
}
function _encryptSend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}/encrypt{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyOperationsParametersSerializer)(parameters)
  });
}
async function _encryptDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyOperationResultDeserializer)(result.body);
}
async function encrypt(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _encryptSend(context, keyName, keyVersion, parameters, options);
  return _encryptDeserialize(result);
}
function _restoreKeySend(context, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/restore{?api%2Dversion}",
    {
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyRestoreParametersSerializer)(parameters)
  });
}
async function _restoreKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function restoreKey(context, parameters, options = { requestOptions: {} }) {
  const result = await _restoreKeySend(context, parameters, options);
  return _restoreKeyDeserialize(result);
}
function _backupKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/backup{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _backupKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.backupKeyResultDeserializer)(result.body);
}
async function backupKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _backupKeySend(context, keyName, options);
  return _backupKeyDeserialize(result);
}
function _getKeysSend(context, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys{?api%2Dversion,maxresults}",
    {
      "api%2Dversion": context.apiVersion,
      maxresults: options?.maxresults
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getKeysDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models._keyListResultDeserializer)(result.body);
}
function getKeys(context, options = { requestOptions: {} }) {
  return (0, import_pagingHelpers.buildPagedAsyncIterator)(
    context,
    () => _getKeysSend(context, options),
    _getKeysDeserialize,
    ["200"],
    { itemName: "value", nextLinkName: "nextLink" }
  );
}
function _getKeyVersionsSend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/versions{?api%2Dversion,maxresults}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion,
      maxresults: options?.maxresults
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getKeyVersionsDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models._keyListResultDeserializer)(result.body);
}
function getKeyVersions(context, keyName, options = { requestOptions: {} }) {
  return (0, import_pagingHelpers.buildPagedAsyncIterator)(
    context,
    () => _getKeyVersionsSend(context, keyName, options),
    _getKeyVersionsDeserialize,
    ["200"],
    { itemName: "value", nextLinkName: "nextLink" }
  );
}
function _getKeySend(context, keyName, keyVersion, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).get({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _getKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function getKey(context, keyName, keyVersion, options = { requestOptions: {} }) {
  const result = await _getKeySend(context, keyName, keyVersion, options);
  return _getKeyDeserialize(result);
}
function _updateKeySend(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/{key-version}{?api%2Dversion}",
    {
      "key-name": keyName,
      "key-version": keyVersion,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).patch({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyUpdateParametersSerializer)(parameters)
  });
}
async function _updateKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function updateKey(context, keyName, keyVersion, parameters, options = { requestOptions: {} }) {
  const result = await _updateKeySend(context, keyName, keyVersion, parameters, options);
  return _updateKeyDeserialize(result);
}
function _deleteKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).delete({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _deleteKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.deletedKeyBundleDeserializer)(result.body);
}
async function deleteKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _deleteKeySend(context, keyName, options);
  return _deleteKeyDeserialize(result);
}
function _importKeySend(context, keyName, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).put({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyImportParametersSerializer)(parameters)
  });
}
async function _importKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function importKey(context, keyName, parameters, options = { requestOptions: {} }) {
  const result = await _importKeySend(context, keyName, parameters, options);
  return _importKeyDeserialize(result);
}
function _rotateKeySend(context, keyName, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/rotate{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    }
  });
}
async function _rotateKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function rotateKey(context, keyName, options = { requestOptions: {} }) {
  const result = await _rotateKeySend(context, keyName, options);
  return _rotateKeyDeserialize(result);
}
function _createKeySend(context, keyName, parameters, options = { requestOptions: {} }) {
  const path = (0, import_urlTemplate.expandUrlTemplate)(
    "/keys/{key-name}/create{?api%2Dversion}",
    {
      "key-name": keyName,
      "api%2Dversion": context.apiVersion
    },
    {
      allowReserved: options?.requestOptions?.skipUrlEncoding
    }
  );
  return context.path(path).post({
    ...(0, import_core_client.operationOptionsToRequestParameters)(options),
    contentType: "application/json",
    headers: {
      accept: "application/json",
      ...options.requestOptions?.headers
    },
    body: (0, import_models.keyCreateParametersSerializer)(parameters)
  });
}
async function _createKeyDeserialize(result) {
  const expectedStatuses = ["200"];
  if (!expectedStatuses.includes(result.status)) {
    const error = (0, import_core_client.createRestError)(result);
    error.details = (0, import_models.keyVaultErrorDeserializer)(result.body);
    throw error;
  }
  return (0, import_models.keyBundleDeserializer)(result.body);
}
async function createKey(context, keyName, parameters, options = { requestOptions: {} }) {
  const result = await _createKeySend(context, keyName, parameters, options);
  return _createKeyDeserialize(result);
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  _backupKeyDeserialize,
  _backupKeySend,
  _createKeyDeserialize,
  _createKeySend,
  _decryptDeserialize,
  _decryptSend,
  _deleteKeyDeserialize,
  _deleteKeySend,
  _encryptDeserialize,
  _encryptSend,
  _getDeletedKeyDeserialize,
  _getDeletedKeySend,
  _getDeletedKeysDeserialize,
  _getDeletedKeysSend,
  _getKeyAttestationDeserialize,
  _getKeyAttestationSend,
  _getKeyDeserialize,
  _getKeyRotationPolicyDeserialize,
  _getKeyRotationPolicySend,
  _getKeySend,
  _getKeyVersionsDeserialize,
  _getKeyVersionsSend,
  _getKeysDeserialize,
  _getKeysSend,
  _getRandomBytesDeserialize,
  _getRandomBytesSend,
  _importKeyDeserialize,
  _importKeySend,
  _purgeDeletedKeyDeserialize,
  _purgeDeletedKeySend,
  _recoverDeletedKeyDeserialize,
  _recoverDeletedKeySend,
  _releaseDeserialize,
  _releaseSend,
  _restoreKeyDeserialize,
  _restoreKeySend,
  _rotateKeyDeserialize,
  _rotateKeySend,
  _signDeserialize,
  _signSend,
  _unwrapKeyDeserialize,
  _unwrapKeySend,
  _updateKeyDeserialize,
  _updateKeyRotationPolicyDeserialize,
  _updateKeyRotationPolicySend,
  _updateKeySend,
  _verifyDeserialize,
  _verifySend,
  _wrapKeyDeserialize,
  _wrapKeySend,
  backupKey,
  createKey,
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
//# sourceMappingURL=operations.js.map
