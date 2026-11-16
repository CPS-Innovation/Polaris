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
var keyVaultAuthenticationPolicy_exports = {};
__export(keyVaultAuthenticationPolicy_exports, {
  keyVaultAuthenticationPolicy: () => keyVaultAuthenticationPolicy,
  keyVaultAuthenticationPolicyName: () => keyVaultAuthenticationPolicyName
});
module.exports = __toCommonJS(keyVaultAuthenticationPolicy_exports);
var import_parseWWWAuthenticate = require("./parseWWWAuthenticate.js");
var import_tokenCycler = require("./tokenCycler.js");
var import_logger = require("./logger.js");
function verifyChallengeResource(scope, request) {
  let scopeAsUrl;
  try {
    scopeAsUrl = new URL(scope);
  } catch (e) {
    throw new Error(`The challenge contains invalid scope '${scope}'`);
  }
  const requestUrl = new URL(request.url);
  if (!requestUrl.hostname.endsWith(`.${scopeAsUrl.hostname}`)) {
    throw new Error(
      `The challenge resource '${scopeAsUrl.hostname}' does not match the requested domain. Set disableChallengeResourceVerification to true in your client options to disable. See https://aka.ms/azsdk/blog/vault-uri for more information.`
    );
  }
}
const keyVaultAuthenticationPolicyName = "keyVaultAuthenticationPolicy";
function keyVaultAuthenticationPolicy(credential, options = {}) {
  const { disableChallengeResourceVerification } = options;
  let challengeState = { status: "none" };
  const getAccessToken = (0, import_tokenCycler.createTokenCycler)(credential);
  function requestToOptions(request) {
    return {
      abortSignal: request.abortSignal,
      requestOptions: {
        timeout: request.timeout > 0 ? request.timeout : void 0
      },
      tracingOptions: request.tracingOptions
    };
  }
  async function authorizeRequest(request) {
    const requestOptions = requestToOptions(request);
    switch (challengeState.status) {
      case "none":
        challengeState = {
          status: "started",
          originalBody: request.body
        };
        request.body = null;
        break;
      case "started":
        break;
      // Retry, we should not overwrite the original body
      case "complete": {
        const token = await getAccessToken(challengeState.scopes, {
          ...requestOptions,
          enableCae: true,
          tenantId: challengeState.tenantId
        });
        if (token) {
          request.headers.set("authorization", `Bearer ${token.token}`);
        }
        break;
      }
    }
  }
  async function handleChallenge(request, response, next) {
    if (response.status !== 401) {
      return response;
    }
    if (request.body === null && challengeState.status === "started") {
      request.body = challengeState.originalBody;
    }
    const getTokenOptions = requestToOptions(request);
    const challenge = response.headers.get("WWW-Authenticate");
    if (!challenge) {
      import_logger.logger.warning(
        "keyVaultAuthentication policy encountered a 401 response without a corresponding WWW-Authenticate header. This is unexpected. Not handling the 401 response."
      );
      return response;
    }
    const parsedChallenge = (0, import_parseWWWAuthenticate.parseWWWAuthenticateHeader)(challenge);
    const scope = parsedChallenge.resource ? parsedChallenge.resource + "/.default" : parsedChallenge.scope;
    if (!scope) {
      return response;
    }
    if (!disableChallengeResourceVerification) {
      verifyChallengeResource(scope, request);
    }
    const accessToken = await getAccessToken([scope], {
      ...getTokenOptions,
      enableCae: true,
      tenantId: parsedChallenge.tenantId
    });
    if (!accessToken) {
      return response;
    }
    request.headers.set("Authorization", `Bearer ${accessToken.token}`);
    challengeState = {
      status: "complete",
      scopes: [scope],
      tenantId: parsedChallenge.tenantId
    };
    return next(request);
  }
  async function handleCaeChallenge(request, response, next) {
    if (challengeState.status !== "complete") {
      return response;
    }
    if (response.status !== 401) {
      return response;
    }
    const getTokenOptions = requestToOptions(request);
    const challenge = response.headers.get("WWW-Authenticate");
    if (!challenge) {
      return response;
    }
    const { claims: base64EncodedClaims, error } = (0, import_parseWWWAuthenticate.parseWWWAuthenticateHeader)(challenge);
    if (error !== "insufficient_claims" || base64EncodedClaims === void 0) {
      return response;
    }
    const claims = atob(base64EncodedClaims);
    const accessToken = await getAccessToken(challengeState.scopes, {
      ...getTokenOptions,
      enableCae: true,
      tenantId: challengeState.tenantId,
      claims
    });
    request.headers.set("Authorization", `Bearer ${accessToken.token}`);
    return next(request);
  }
  async function sendRequest(request, next) {
    await authorizeRequest(request);
    let response = await next(request);
    response = await handleChallenge(request, response, next);
    response = await handleCaeChallenge(request, response, next);
    return response;
  }
  return {
    name: keyVaultAuthenticationPolicyName,
    sendRequest
  };
}
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  keyVaultAuthenticationPolicy,
  keyVaultAuthenticationPolicyName
});
//# sourceMappingURL=keyVaultAuthenticationPolicy.js.map
