async function polarisAuthRedirect(r) {
  const redirectHostAddress = r.variables.redirectHostAddress ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  const referer = encodeURIComponent(r.headersIn.Referer ?? "")
  const polarisUiUrl = encodeURIComponent(r.args["polaris-ui-url"] ?? "")
  const q = r.args["q"] ?? ""
  r.return(
    302,
    `${redirectHostAddress}/api/init?cookie=${cookie}&referer=${referer}&polaris-ui-url=${polarisUiUrl}&q=${q}`
  )
}

function taskListAuthRedirect(r) {
  const taskListHostAddress = r.variables['taskListHostAddress'] ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  r.return(
      302,
      `${taskListHostAddress}/POCWorkCaseManagementGOVUK/RedirectPage?Cookie=${cookie}`
  )
}

export default { polarisAuthRedirect, taskListAuthRedirect }
