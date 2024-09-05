function _legacyHandoverFormatShim(r) {
  // If we have no r param then we assume we are processing a legacy handover from the /polaris endpoint.
  // The CMS P button has no concept of the r param and assumes this endpoint forwards on to CWA domain.
  // So lets coerce the legacy format to the standard format by creating an r param if one does not exist.
  // Note 1: the expected incoming params in the legacy case are q and referer.
  // Note 2: we use a relative URL rather than a fully-qualified URL as the proxy runs under multiple names
  //  e.g. https://polaris-cmsproxy.azurewebsites.net/ and https://polaris.cps.gov.uk/
  return {
    variables: { ...r.variables },
    args: {
      ...r.args,
      r:
        r.args["r"] ??
        `/auth-refresh-inbound?q=${r.args["q"]}&referer=${r.args["referer"]}`,
    },
  }
}

async function appAuthRedirect(r) {
  r = _legacyHandoverFormatShim(r)

  const redirectUrl = r.args["r"]
  const isWhitelisted = (r.variables.whitelistedUrls ?? "")
    .split(",")
    .some((url) => redirectUrl.startsWith(url))

  if (isWhitelisted) {
    r.return(
      302,
      `${redirectUrl}${
        redirectUrl.includes("?") ? "&" : "?"
      }__cc=${encodeURIComponent(r.args["cookie"] ?? "")}`
    )
  } else {
    r.return(403)
  }
}

// This is a simulation of the https://cms.cps.gov.uk/polaris endpoint.
//  Primarily useful when users are using CMS delivered through this proxy. In this use case, users are on this proxy
//  domain when using CMS.  We inject a P button and simulated the prod /polaris handover endpoint using this function.
async function polarisAuthRedirect(r) {
  const redirectHostAddress = r.variables.redirectHostAddress ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  const referer = encodeURIComponent(r.headersIn.Referer ?? "")
  const polarisUiUrl = encodeURIComponent(r.args["polaris-ui-url"] ?? "")
  const q = r.args["q"] ?? ""

  const redirect = r.args["r"] ?? ""

  r.return(
    302,
    `${redirectHostAddress}/init?cookie=${cookie}&referer=${referer}&polaris-ui-url=${polarisUiUrl}&q=${q}&r=${redirect}`
  )
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
