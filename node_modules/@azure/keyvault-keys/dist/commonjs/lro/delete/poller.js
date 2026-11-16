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
var poller_exports = {};
__export(poller_exports, {
  DeleteKeyPoller: () => DeleteKeyPoller
});
module.exports = __toCommonJS(poller_exports);
var import_operation = require("./operation.js");
var import_keyVaultKeyPoller = require("../keyVaultKeyPoller.js");
class DeleteKeyPoller extends import_keyVaultKeyPoller.KeyVaultKeyPoller {
  constructor(options) {
    const { client, name, operationOptions, intervalInMs = 2e3, resumeFrom } = options;
    let state;
    if (resumeFrom) {
      state = JSON.parse(resumeFrom).state;
    }
    const operation = new import_operation.DeleteKeyPollOperation(
      {
        ...state,
        name
      },
      client,
      operationOptions
    );
    super(operation);
    this.intervalInMs = intervalInMs;
  }
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  DeleteKeyPoller
});
//# sourceMappingURL=poller.js.map
