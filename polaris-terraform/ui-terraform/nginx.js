async function polarisAuthRedirect(r) {
  const redirectHostAddress = r.variables.redirectHostAddress ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  const referer = encodeURIComponent(r.headersIn.Referer ?? "")
  const polarisUiUrl = encodeURIComponent(r.args["polaris-ui-url"] ?? "")
  r.return(
    302,
    `${redirectHostAddress}/api/init?cookie=${cookie}&referer=${referer}&polaris-ui-url=${polarisUiUrl}`
  )
}

export default { polarisAuthRedirect }
