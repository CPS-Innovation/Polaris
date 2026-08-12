// Shared primitive — IE / Edge negotiation state.
//
// Derives $ieaction, the browser-mode string that the per-location gates compare
// against. This replaces the server-preamble `if`-ladder that used to live in
// nginx.conf; wired via `js_set $ieaction ieMode.ieAction;` at server scope.
//
// Behaviour-identical to the ladder: builds "{ie|nonie}+{configurable|nonconfigurable}+"
// from the User-Agent (Trident => ie) and the X-InternetExplorerModeConfigurable
// header (value "1" => configurable). Absent headers => nonie / nonconfigurable,
// exactly as the ladder's `!= 1` / `!~ Trident` branches produced.
//
// Kept as its own common module (not folded into cms-detection.js) — different
// concern (browser mode, not CMS environment). Cross-cutting: consumed by
// auth-handover, cms-proxy and global-components.
function ieAction(r) {
  const ie = /Trident/i.test(r.headersIn["User-Agent"] || "") ? "ie" : "nonie";
  const configurable =
    (r.headersIn["X-InternetExplorerModeConfigurable"] || "") === "1"
      ? "configurable"
      : "nonconfigurable";
  return ie + "+" + configurable + "+";
}

// Enforce the browser mode a js_content endpoint needs, in one call — the njs
// equivalent of the per-location `if ($ieaction = …)` gate, for handlers that own
// their response (proxy_pass locations can't use this; they keep the conf gate).
// Computes the state locally, so the caller needs no $ieaction variable.
//   want   — "ie"  (IE Mode Desired)  or  "edge" (Edge / non-IE desired)
//   reject — when true, 402 if the browser is in the wrong mode AND cannot switch
//            (nonconfigurable). Some endpoints (e.g. /polaris) proceed instead.
// Returns true if it emitted a response (302 to coerce, or 402) — the caller must
// then `return`. Returns false when the browser is already in the wanted mode.
function coerce(r, want, reject) {
  const state = ieAction(r);
  const inIe = state.indexOf("nonie") !== 0; // starts "ie+" (not "nonie+")
  const configurable = state.indexOf("nonconfigurable") === -1;
  const wrongMode = want === "ie" ? !inIe : inIe;
  if (!wrongMode) {
    return false; // already in the wanted mode — proceed
  }
  if (configurable) {
    r.headersOut["X-InternetExplorerMode"] = want === "ie" ? "1" : "0";
    r.return(
      302,
      process.env.WEBSITE_SCHEME + "://" + r.variables.host + r.variables.request_uri,
    );
    return true;
  }
  if (reject) {
    r.return(
      402,
      want === "ie"
        ? "requires Internet Explorer mode"
        : "requires non-internet explorer mode",
    );
    return true;
  }
  return false; // wrong mode, can't switch, not rejecting — proceed
}

export default { ieAction, coerce };
