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
  DeleteKeyPollOperation: () => DeleteKeyPollOperation
});
module.exports = __toCommonJS(operation_exports);
var import_tracing = require("../../tracing.js");
var import_transformations = require("../../transformations.js");
var import_keyVaultKeyPoller = require("../keyVaultKeyPoller.js");
class DeleteKeyPollOperation extends import_keyVaultKeyPoller.KeyVaultKeyPollOperation {
  constructor(state, client, operationOptions = {}) {
    super(state, { cancelMessage: "Canceling the deletion of a key is not supported." });
    this.state = state;
    this.client = client;
    this.operationOptions = operationOptions;
  }
  state;
  client;
  operationOptions;
  /**
   * Sends a delete request for the given Key Vault Key's name to the Key Vault service.
   * Since the Key Vault Key won't be immediately deleted, we have {@link beginDeleteKey}.
   */
  deleteKey(name, options = {}) {
    return import_tracing.tracingClient.withSpan("DeleteKeyPoller.deleteKey", options, async (updatedOptions) => {
      const response = await this.client.deleteKey(name, updatedOptions);
      return (0, import_transformations.getKeyFromKeyBundle)(response);
    });
  }
  /**
   * The getDeletedKey method returns the specified deleted key along with its properties.
   * This operation requires the keys/get permission.
   */
  getDeletedKey(name, options = {}) {
    return import_tracing.tracingClient.withSpan(
      "DeleteKeyPoller.getDeletedKey",
      options,
      async (updatedOptions) => {
        const response = await this.client.getDeletedKey(name, updatedOptions);
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
    if (options.abortSignal) {
      this.operationOptions.abortSignal = options.abortSignal;
    }
    if (!state.isStarted) {
      const deletedKey = await this.deleteKey(name, this.operationOptions);
      state.isStarted = true;
      state.result = deletedKey;
      if (!deletedKey.properties.recoveryId) {
        state.isCompleted = true;
      }
    }
    if (!state.isCompleted) {
      try {
        state.result = await this.getDeletedKey(name, this.operationOptions);
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
  DeleteKeyPollOperation
});
//# sourceMappingURL=operation.js.map
