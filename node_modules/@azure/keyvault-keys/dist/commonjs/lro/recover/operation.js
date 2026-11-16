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
var operation_exports = {};
__export(operation_exports, {
  RecoverDeletedKeyPollOperation: () => RecoverDeletedKeyPollOperation
});
module.exports = __toCommonJS(operation_exports);
var import_tracing = require("../../tracing.js");
var import_transformations = require("../../transformations.js");
var import_keyVaultKeyPoller = require("../keyVaultKeyPoller.js");
class RecoverDeletedKeyPollOperation extends import_keyVaultKeyPoller.KeyVaultKeyPollOperation {
  constructor(state, client, operationOptions = {}) {
    super(state, { cancelMessage: "Canceling the recovery of a deleted key is not supported." });
    this.state = state;
    this.client = client;
    this.operationOptions = operationOptions;
  }
  state;
  client;
  operationOptions;
  /**
   * The getKey method gets a specified key and is applicable to any key stored in Azure Key Vault.
   * This operation requires the keys/get permission.
   */
  getKey(name, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RecoverDeleteKeyPoller.getKey",
      options,
      async (updatedOptions) => {
        const response = await this.client.getKey(
          name,
          updatedOptions?.version || "",
          updatedOptions
        );
        return (0, import_transformations.getKeyFromKeyBundle)(response);
      }
    );
  }
  /**
   * Sends a request to recover a deleted Key Vault Key based on the given name.
   * Since the Key Vault Key won't be immediately recover the deleted key, we have {@link beginRecoverDeletedKey}.
   */
  async recoverDeletedKey(name, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "RecoverDeletedKeyPoller.recoverDeleteKey",
      options,
      async (updatedOptions) => {
        const response = await this.client.recoverDeletedKey(name, updatedOptions);
        return (0, import_transformations.getKeyFromKeyBundle)(response);
      }
    );
  }
  /**
   * Reaches to the service and updates the delete key's poll operation.
   */
  async update(options = {}) {
    const state = this.state;
    const { name } = state;
    const operationOptions = this.operationOptions;
    if (options.abortSignal) {
      operationOptions.abortSignal = options.abortSignal;
    }
    if (!state.isStarted) {
      try {
        state.result = await this.getKey(name, operationOptions);
        state.isCompleted = true;
      } catch {
      }
      if (!state.isCompleted) {
        state.result = await this.recoverDeletedKey(name, operationOptions);
        state.isStarted = true;
      }
    }
    if (!state.isCompleted) {
      try {
        state.result = await this.getKey(name, operationOptions);
        state.isCompleted = true;
      } catch (error) {
        if (error.statusCode === 403) {
          state.isCompleted = true;
        } else if (error.statusCode !== 404) {
          state.error = error;
          state.isCompleted = true;
          throw error;
        }
      }
    }
    return this;
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  RecoverDeletedKeyPollOperation
});
//# sourceMappingURL=operation.js.map
