async function fetch(r) {
  const endpoint = process.env.API_ENDPOINT ?? ""
  const cookie = encodeURIComponent(r.headersIn.Cookie ?? "")
  const referer = encodeURIComponent(r.headersIn.Referer ?? "")
  const q = encodeURIComponent(r.args.q ?? "")
  r.return(
    302,
    `https://${endpoint}/init?cookie=${cookie}&referer=${referer}&q=${q}`
  )
}
export default { fetch }
