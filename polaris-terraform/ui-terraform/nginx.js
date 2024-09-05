import qs from "querystring"

function _argsShim(args) {
  if (args["r"]) {
    return args
  }
  // If we have no r param then we assume we are processing a legacy handover from the /polaris endpoint.
  // The CMS P button has no concept of the r param and assumes this endpoint forwards on to CWA domain.
  // So lets coerce the legacy format to the standard format by creating an r param if one does not exist.
  // Note 1: the expected incoming params in the legacy case are q and referer.
  // Note 2: we use a relative URL rather than a fully-qualified URL as the proxy runs under multiple names
  //  e.g. https://polaris-cmsproxy.azurewebsites.net/ and https://polaris.cps.gov.uk/

  const serializedArgs = qs.stringify(args)
  const clonedArgsToMutate = qs.parse(serializedArgs)
  delete clonedArgsToMutate["cookie"]
  // do not serialize cookie into our manufactured r param because cookie will be attached as the __cc param later on
  const queryStringWithoutCookie = qs.stringify(clonedArgsToMutate)

  const clonedArgs = qs.parse(serializedArgs)
  clonedArgs["r"] = `/auth-refresh-inbound?${queryStringWithoutCookie}`
  return clonedArgs
}

function appAuthRedirect(r) {
  const args = _argsShim(r.args)

  const whitelistedUrls = process.env.AUTH_HANDOVER_WHITELIST ?? ""
  const redirectUrl = args["r"]
  const isWhitelisted = whitelistedUrls
    .split(",")
    .some((url) => redirectUrl.startsWith(url))

  if (isWhitelisted) {
    r.return(
      302,
      `${redirectUrl}${
        redirectUrl.includes("?") ? "&" : "?"
      }__cc=${encodeURIComponent(args["cookie"] ?? "")}`
    )
  } else {
    r.return(
      403,
      `HTTP Status 403: this deployment of the /init endpoint will only accept requests with r query parameters that start with one of the following strings: ${whitelistedUrls}`
    )
  }
}

// This is a simulation of the https://cms.cps.gov.uk/polaris endpoint.
//  Primarily useful when users are using CMS delivered through this proxy. In this use case, users are on this proxy
//  domain when using CMS.  We inject a P button and simulated the prod /polaris handover endpoint using this function.
function polarisAuthRedirect(r) {
  const serializedArgs = qs.stringify(r.args)
  const clonedArgs = qs.parse(serializedArgs)
  clonedArgs.cookie = r.headersIn.Cookie
  clonedArgs.referer = r.headersIn.Referer

  const querystring = qs.stringify(clonedArgs)

  r.return(302, `/init?${querystring}`)
}

function taskListAuthRedirect(r) {
  const taskListHostAddress = r.variables["taskListHostAddress"] ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  r.return(
    302,
    `${taskListHostAddress}/WorkManagementApp/Redirect?Cookie=${cookie}`
  )
}

export default { polarisAuthRedirect, taskListAuthRedirect, appAuthRedirect }
