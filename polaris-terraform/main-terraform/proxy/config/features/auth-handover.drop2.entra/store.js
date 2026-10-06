// ---------------------------------------------------------------------------
// CMS-auth deposit store — the SWAP-OUT SEAM, now backed by MDS.
//
// One narrow, backend-agnostic contract:
//
//   scope                            the Entra scope the backend's bearer token must carry
//   deposit(payload, bearer) -> { ok, diag }
//     payload: { cookies, token }    (the CMS cookie header + the CMS modern token)
//     bearer:  an access token for `scope`, which the drop2 callback obtains by an
//              on-behalf-of swap of the user's neutral token (see auth-handover.drop2.entra.js)
//
// The callback asks the store which scope it needs, swaps for it, and calls deposit(); it never
// learns which backend ran. This backend is MDS: PUT <WM_MDS_BASE_URL>cms-auth-store with the
// bearer token, plus the same x-functions-key the global-components /global-components/api route
// sends (we read the SAME two settings, WM_MDS_BASE_URL + WM_MDS_ACCESS_KEY, rather than calling
// that route: it strips Authorization, and a loopback would leave and re-enter the proxy).
//
// MDS identifies the user from the token (`oid`) — the store sends no identity of its own. Today
// MDS only reads `oid`; full token validation arrives later as MDS middleware, which this token's
// shape (v2, aud = the MDS app, scp = ENTRA_MDS_SCOPE, the user's oid) is built to pass.
//
// History: the previous backend was Azure Table Storage (SharedKeyLite, ENTRA_STORAGE_*), keyed
// by an OID taken from a validated id_token. Replaced 2026-10 — see git history / QUIRKS E10.
//
// ISOLATED / NEW-GEN: only ever reached when drop2 is armed. No imports, so it swaps/deletes cleanly.
// ---------------------------------------------------------------------------

// Config from app settings. WM_MDS_* are the proxy's existing MDS settings (terraform-managed, Key
// Vault for the key). ENTRA_MDS_SCOPE has no baked default (the api:// prefix differs per env, e.g.
// QA api://fa-wm-app-ddei-staging/full_scope). Missing => "" => the swap/deposit fails and drop2
// degrades to drop1 behaviour; nothing can stop nginx booting (process.env, not envsubst).
const SCOPE = process.env.ENTRA_MDS_SCOPE || "";
const MDS_BASE_URL = process.env.WM_MDS_BASE_URL || "";
const MDS_ACCESS_KEY = process.env.WM_MDS_ACCESS_KEY || "";
const MDS_PATH = "cms-auth-store";

// Decided 2026-10-06: the CMS session's real expiry is unknowable to us (.CMSAUTH is an encrypted
// forms ticket; the modern token is a bare GUID), and MDS does not use expiryTime today. A fixed,
// obviously-artificial value. NB: if MDS ever starts honouring expiryTime, a past date means every
// record is born expired — change this to a policy value (e.g. now + N hours) at that point.
const EXPIRY_TIME = "2000-01-01T00:00:00Z";

// base (e.g. "https://fa-wm-app-ddei-staging.azurewebsites.net/api/") + path, tolerating either
// trailing-slash convention.
function _url() {
  return MDS_BASE_URL.replace(/\/+$/, "") + "/" + MDS_PATH;
}

function _host(url) {
  const m = url.match(/^https?:\/\/([^/]+)/i);
  return m ? m[1] : "";
}

async function mdsDeposit(payload, bearer) {
  if (!MDS_BASE_URL) return { ok: false, diag: "no-mds-base-url" };
  if (!bearer) return { ok: false, diag: "no-bearer" };

  const url = _url();
  const headers = {
    Authorization: "Bearer " + bearer,
    "Content-Type": "application/json",
    Host: _host(url),
  };
  if (MDS_ACCESS_KEY) headers["x-functions-key"] = MDS_ACCESS_KEY;

  try {
    const resp = await ngx.fetch(url, {
      method: "PUT",
      headers: headers,
      body: JSON.stringify({
        cookies: payload.cookies,
        token: payload.token,
        expiryTime: EXPIRY_TIME,
      }),
    });
    if (!resp.ok) {
      const errText = await resp.text();
      ngx.log(ngx.ERR, "mds cms-auth-store PUT failed: " + resp.status + " " + errText);
      return { ok: false, diag: "HTTP " + resp.status + " " + errText.substring(0, 120) };
    }
    return { ok: true, diag: "ok" };
  } catch (e) {
    ngx.log(ngx.ERR, "mds cms-auth-store PUT error: " + String(e));
    return { ok: false, diag: String(e) };
  }
}

// THE SEAM. Swap this one binding (and `scope`) to migrate backends. The callback imports
// `deposit` + `scope`, not the concrete backend.
const deposit = mdsDeposit;

export default {
  scope: SCOPE,
  deposit,
  mdsDeposit,
  // exposed for the unit test (production only calls `deposit`):
  __test: { url: _url, EXPIRY_TIME: EXPIRY_TIME },
};
