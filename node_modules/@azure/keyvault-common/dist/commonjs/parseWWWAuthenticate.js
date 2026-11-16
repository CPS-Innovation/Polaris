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
var parseWWWAuthenticate_exports = {};
__export(parseWWWAuthenticate_exports, {
  parseWWWAuthenticateHeader: () => parseWWWAuthenticateHeader
});
module.exports = __toCommonJS(parseWWWAuthenticate_exports);
const validWWWAuthenticateProperties = [
  "authorization",
  "authorization_url",
  "resource",
  "scope",
  "tenantId",
  "claims",
  "error"
];
function parseWWWAuthenticateHeader(headerValue) {
  const pairDelimiter = /,? +/;
  const parsed = headerValue.split(pairDelimiter).reduce((kvPairs, p) => {
    if (p.match(/\w="/)) {
      const [key, ...value] = p.split("=");
      if (validWWWAuthenticateProperties.includes(key)) {
        return { ...kvPairs, [key]: value.join("=").slice(1, -1) };
      }
    }
    return kvPairs;
  }, {});
  if (parsed.authorization) {
    try {
      const tenantId = new URL(parsed.authorization).pathname.substring(1);
      if (tenantId) {
        parsed.tenantId = tenantId;
      }
    } catch (_) {
      throw new Error(`The challenge authorization URI '${parsed.authorization}' is invalid.`);
    }
  }
  return parsed;
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  parseWWWAuthenticateHeader
});
//# sourceMappingURL=parseWWWAuthenticate.js.map
