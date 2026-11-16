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
var keyVaultKeyPoller_exports = {};
__export(keyVaultKeyPoller_exports, {
  KeyVaultKeyPollOperation: () => KeyVaultKeyPollOperation,
  KeyVaultKeyPoller: () => KeyVaultKeyPoller
});
module.exports = __toCommonJS(keyVaultKeyPoller_exports);
var import_core_util = require("@azure/core-util");
var import_core_lro = require("@azure/core-lro");
class KeyVaultKeyPoller extends import_core_lro.Poller {
  /**
   * Defines how much time the poller is going to wait before making a new request to the service.
   */
  intervalInMs = 2e3;
  /**
   * The method used by the poller to wait before attempting to update its operation.
   */
  async delay() {
    return (0, import_core_util.delay)(this.intervalInMs);
  }
}
class KeyVaultKeyPollOperation {
  constructor(state, options = {}) {
    this.state = state;
    if (options.cancelMessage) {
      this.cancelMessage = options.cancelMessage;
    }
  }
  state;
  cancelMessage = "";
  /**
   * Meant to reach to the service and update the Poller operation.
   */
  async update() {
    throw new Error("Operation not supported.");
  }
  /**
   * Meant to reach to the service and cancel the Poller operation.
   */
  async cancel() {
    throw new Error(this.cancelMessage);
  }
  /**
   * Serializes the Poller operation.
   */
  toString() {
    return JSON.stringify({
      state: this.state
    });
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  KeyVaultKeyPollOperation,
  KeyVaultKeyPoller
});
//# sourceMappingURL=keyVaultKeyPoller.js.map
