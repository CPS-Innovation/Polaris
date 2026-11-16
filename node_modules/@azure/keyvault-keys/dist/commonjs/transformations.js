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
var transformations_exports = {};
__export(transformations_exports, {
  getDeletedKeyFromDeletedKeyItem: () => getDeletedKeyFromDeletedKeyItem,
  getKeyFromKeyBundle: () => getKeyFromKeyBundle,
  getKeyPropertiesFromKeyItem: () => getKeyPropertiesFromKeyItem,
  keyRotationTransformations: () => keyRotationTransformations,
  mapPagedAsyncIterable: () => mapPagedAsyncIterable
});
module.exports = __toCommonJS(transformations_exports);
var import_identifier = require("./identifier.js");
function getKeyFromKeyBundle(bundle) {
  const keyBundle = bundle;
  const deletedKeyBundle = bundle;
  const parsedId = (0, import_identifier.parseKeyVaultKeyIdentifier)(keyBundle.key.kid);
  const attributes = keyBundle.attributes || {};
  const resultObject = {
    key: keyBundle.key,
    id: keyBundle.key ? keyBundle.key.kid : void 0,
    name: parsedId.name,
    keyOperations: keyBundle.key ? keyBundle.key.keyOps : void 0,
    keyType: keyBundle.key ? keyBundle.key.kty : void 0,
    properties: {
      tags: keyBundle.tags,
      enabled: attributes.enabled,
      notBefore: attributes.notBefore,
      expiresOn: attributes.expires,
      createdOn: attributes.created,
      updatedOn: attributes.updated,
      recoverableDays: attributes.recoverableDays,
      recoveryLevel: attributes.recoveryLevel,
      exportable: attributes.exportable,
      releasePolicy: keyBundle.releasePolicy,
      hsmPlatform: attributes.hsmPlatform,
      vaultUrl: parsedId.vaultUrl,
      version: parsedId.version,
      name: parsedId.name,
      managed: keyBundle.managed,
      id: keyBundle.key ? keyBundle.key.kid : void 0
    }
  };
  if (deletedKeyBundle.recoveryId) {
    resultObject.properties.recoveryId = deletedKeyBundle.recoveryId;
    resultObject.properties.scheduledPurgeDate = deletedKeyBundle.scheduledPurgeDate;
    resultObject.properties.deletedOn = deletedKeyBundle.deletedDate;
  }
  if (attributes.attestation) {
    resultObject.properties.attestation = attributes.attestation;
  }
  return resultObject;
}
function getDeletedKeyFromDeletedKeyItem(keyItem) {
  const commonProperties = getKeyPropertiesFromKeyItem(keyItem);
  return {
    key: {
      kid: keyItem.kid
    },
    id: keyItem.kid,
    name: commonProperties.name,
    properties: {
      ...commonProperties,
      recoveryId: keyItem.recoveryId,
      scheduledPurgeDate: keyItem.scheduledPurgeDate,
      deletedOn: keyItem.deletedDate
    }
  };
}
function getKeyPropertiesFromKeyItem(keyItem) {
  const parsedId = (0, import_identifier.parseKeyVaultKeyIdentifier)(keyItem.kid);
  const attributes = keyItem.attributes || {};
  const resultObject = {
    createdOn: attributes.created,
    enabled: attributes?.enabled,
    expiresOn: attributes?.expires,
    id: keyItem.kid,
    managed: keyItem.managed,
    name: parsedId.name,
    notBefore: attributes?.notBefore,
    recoverableDays: attributes?.recoverableDays,
    recoveryLevel: attributes?.recoveryLevel,
    hsmPlatform: attributes?.hsmPlatform,
    tags: keyItem.tags,
    updatedOn: attributes.updated,
    vaultUrl: parsedId.vaultUrl,
    version: parsedId.version
  };
  return resultObject;
}
const actionTypeCaseInsensitiveMapping = {
  rotate: "Rotate",
  notify: "Notify"
};
function getNormalizedActionType(caseInsensitiveActionType) {
  const result = actionTypeCaseInsensitiveMapping[caseInsensitiveActionType.toLowerCase()];
  if (result) {
    return result;
  }
  throw new Error(`Unrecognized action type: ${caseInsensitiveActionType}`);
}
const keyRotationTransformations = {
  propertiesToGenerated: function(parameters) {
    const policy = {
      attributes: {
        expiryTime: parameters.expiresIn
      },
      lifetimeActions: parameters.lifetimeActions?.map((action) => {
        const generatedAction = {
          action: { type: action.action },
          trigger: {}
        };
        if (action.timeAfterCreate) {
          generatedAction.trigger.timeAfterCreate = action.timeAfterCreate;
        }
        if (action.timeBeforeExpiry) {
          generatedAction.trigger.timeBeforeExpiry = action.timeBeforeExpiry;
        }
        return generatedAction;
      })
    };
    return policy;
  },
  generatedToPublic(generated) {
    const policy = {
      id: generated.id,
      createdOn: generated.attributes?.created,
      updatedOn: generated.attributes?.updated,
      expiresIn: generated.attributes?.expiryTime,
      lifetimeActions: generated.lifetimeActions?.map((action) => {
        return {
          action: getNormalizedActionType(action.action.type),
          timeAfterCreate: action.trigger?.timeAfterCreate,
          timeBeforeExpiry: action.trigger?.timeBeforeExpiry
        };
      })
    };
    return policy;
  }
};
function mapPagedAsyncIterable(options, operation, mapper) {
  let iter = void 0;
  return {
    async next() {
      iter ??= operation({ ...options, maxresults: void 0 });
      const result = await iter.next();
      return {
        ...result,
        value: result.value && mapper(result.value)
      };
    },
    [Symbol.asyncIterator]() {
      return this;
    },
    async *byPage(settings) {
      const iteratorByPage = operation({ ...options, maxresults: settings?.maxPageSize }).byPage(
        settings
      );
      for await (const page of iteratorByPage) {
        yield page.map(mapper);
      }
    }
  };
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  getDeletedKeyFromDeletedKeyItem,
  getKeyFromKeyBundle,
  getKeyPropertiesFromKeyItem,
  keyRotationTransformations,
  mapPagedAsyncIterable
});
//# sourceMappingURL=transformations.js.map
