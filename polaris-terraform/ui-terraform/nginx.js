async function appAuthRedirect(r)
{
    const whitelistedUrlsStr = r.variables.whitelistedUrls ?? "";
    const redirect = r.args["r"] ?? "";

    const whitelistedUrls = whitelistedUrlsStr.split(",");
    
    if(!whitelistedUrls.some(url => redirect.startsWith(url))) {
        r.return(403);
        return;
    }

    const cookie = encodeURIComponent(r.args["cookie"] ?? "");
    const symbol = redirect.includes("?") ? "&" : "?";

    r.return(
        302,
        `${redirect}${symbol}__cc=${cookie}`
    )
}

async function polarisAuthRedirect(r)
{
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
