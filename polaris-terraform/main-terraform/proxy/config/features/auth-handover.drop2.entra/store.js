// ---------------------------------------------------------------------------
// CMS-auth deposit store — the SWAP-OUT SEAM.
//
// One narrow, backend-agnostic entry point:
//
//   deposit(oid, payload, tokens) -> { ok, diag }
//     payload: { cookies, modernToken, correlationId, email }  (CMS cookies + modern token)
//     tokens:  { idToken, accessToken }                        (AD tokens in hand at callback)
//
// The auth-handover.drop2.entra callback calls deposit() and never learns which
// backend ran. THIS drop's backend is tableStorageDeposit (Azure Table Storage,
// SharedKeyLite account-key auth). The planned MDS-API migration is a drop-in second
// backend (apiEndpointDeposit: POST the payload with Authorization: Bearer <accessToken>,
// the endpoint validates the token, extracts the OID, and stores) — selected by swapping
// the single `deposit` binding at the bottom of this file, with ZERO change to the auth
// flow. tableStorageDeposit ignores `tokens`; the future backend uses them — the callback
// passes them regardless so the swap needs no signature change.
//
// ISOLATED / NEW-GEN: only ever reached when ENTRA_STORE_ENABLED=true. Self-contained
// (the only import is njs's built-in crypto for the HMAC) so it swaps/deletes cleanly.
// ---------------------------------------------------------------------------

import cryptoModule from "crypto";

// QA defaults baked in (reused from the global-components.cms-auth-v2 reference); the
// account KEY is a secret and stays empty unless supplied as an app setting.
const STORAGE_ACCOUNT = process.env.ENTRA_STORAGE_ACCOUNT || "sacpsglobalcomponents";
const STORAGE_KEY = process.env.ENTRA_STORAGE_KEY || "";
const STORAGE_TABLE = process.env.ENTRA_STORAGE_TABLE || "cmsauth";

// SharedKeyLite signature for Table Storage: HMAC-SHA256 of "<date>\n/<account>/<resource>"
// with the base64-decoded account key, base64-encoded. (Table Storage's lighter scheme —
// no canonicalized headers, unlike SharedKey.)
function _sharedKeyLite(account, key, dateStr, resource) {
  const stringToSign = dateStr + "\n" + "/" + account + "/" + resource;
  const keyBuffer = Buffer.from(key, "base64");
  return (
    "SharedKeyLite " +
    account +
    ":" +
    cryptoModule.createHmac("sha256", keyBuffer).update(stringToSign).digest("base64")
  );
}

// PUT an entity keyed by OID (PartitionKey=<oid>, RowKey='cmsAuth'). PUT is an
// insert-or-REPLACE (idempotent per OID), so a fresh login overwrites the row.
async function tableStorageDeposit(oid, payload, tokens) {
  if (!STORAGE_ACCOUNT || !STORAGE_KEY) {
    return { ok: false, diag: "no-storage-creds" };
  }

  const resource = STORAGE_TABLE + "(PartitionKey='" + oid + "',RowKey='cmsAuth')";
  const url = "https://" + STORAGE_ACCOUNT + ".table.core.windows.net/" + resource;
  const dateStr = new Date().toUTCString();
  const auth = _sharedKeyLite(STORAGE_ACCOUNT, STORAGE_KEY, dateStr, resource);

  const body = JSON.stringify({
    PartitionKey: oid,
    RowKey: "cmsAuth",
    Value: JSON.stringify(payload),
    Email: payload.email || "",
  });

  try {
    const resp = await ngx.fetch(url, {
      method: "PUT",
      headers: {
        Authorization: auth,
        "x-ms-date": dateStr,
        "x-ms-version": "2019-02-02",
        "Content-Type": "application/json",
        Accept: "application/json;odata=nometadata",
        Host: STORAGE_ACCOUNT + ".table.core.windows.net",
      },
      body: body,
    });
    if (!resp.ok) {
      const errText = await resp.text();
      ngx.log(ngx.ERR, "entra store PUT failed: " + resp.status + " " + errText);
      return { ok: false, diag: "HTTP " + resp.status + " " + errText.substring(0, 120) };
    }
    return { ok: true, diag: "ok" };
  } catch (e) {
    ngx.log(ngx.ERR, "entra store PUT error: " + String(e));
    return { ok: false, diag: String(e) };
  }
}

// THE SEAM. Swap this one binding to migrate backends (e.g. `const deposit =
// apiEndpointDeposit`). The callback imports `deposit`, not the concrete backend.
const deposit = tableStorageDeposit;

export default {
  deposit,
  tableStorageDeposit,
  // exposed for the unit test (production only calls `deposit`):
  __test: { sharedKeyLite: _sharedKeyLite },
};
