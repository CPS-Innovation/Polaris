/* cms-auth-v2-client.js
 * Injected into the persistent CMS frameset shell (uaglCMS.aspx <head>), which
 * loads once per session and persists THROUGH login (the login page and the app
 * both load into the shell's frameMain). Two independent concerns, each its own
 * IIFE so a failure in one cannot affect the other:
 *   1. Contact-edit logger + section presence (immediately below) — observes the
 *      Witnesses & details screen, reports which contact is being edited, and (via
 *      the polaris presence relay + a cps.gov.uk cookie bridge) shows an "also
 *      viewing" banner when another user is in the same section.
 *   2. Login -> auth iframe (bottom of file) — on login, spawns the /polaris auth
 *      iframe; its AD callback stashes the id-token in POLARIS localStorage for the
 *      relay to read same-origin. No cross-subdomain cookie hand-off.
 *
 * ---- Concern 1: contact-edit logger ----------------------------------------
 *
 * !! IE MODE / DOCUMENT-MODE 5 (old JScript). DO NOT let Prettier format this !!
 * Trailing commas in object literals or call argument lists are a SYNTAX ERROR
 * here ("SCRIPT1028: Expected identifier, string or number"); the whole file
 * then fails to parse and silently never runs. This file is listed in
 * /.prettierignore for exactly that reason. Also: no const/let/arrow functions,
 * no Array.indexOf/forEach, no String.trim, no JSON, no querySelector, no
 * addEventListener, no MutationObserver. Use var, function declarations,
 * manual loops, document.all/getElementsByTagName and attachEvent.
 *
 * Reports which victim/witness contact's right-hand edit panel is open on the
 * Witnesses & details screen (uaccContactDetails.aspx), and when it stops being
 * open. Two identifiers are reported for the selected person:
 *
 *   personId          - the person/party id. This is hidContactId[index]; the
 *                        SAME value appears for that person's Victim and Witness
 *                        rows, and it is what the URL calls intPersonId.
 *   contactRecorderId - the per-witness contact record id, looked up by personId
 *                        from hidContactRecorderWitnessCache
 *                        ("personId,version,recorderId|..."), falling back to
 *                        hidint64OldContactRecorderIdCSV[index]. Empty for a
 *                        victim who is not also a witness.
 *
 * EVENTS — emitted on transition only, never repeated per poll:
 *   "editing" - a contact's edit panel just became open.
 *   "closed"  - that contact's panel is no longer open (Cancel/OK/deselect, the
 *               frame navigated away, or the shell unloaded).
 * Switching contact A -> B emits "closed" A then "editing" B.
 *
 * TO CALL AN ENDPOINT: every event funnels through ONE function, sendEvent() —
 * see the "OUTPUT SINK" section below. Set ENDPOINT_URL, uncomment one line,
 * and you are done; nothing else in this file needs to change.
 *
 * Alternatively, without editing this file at all, assign a handler at runtime:
 *   window.__ccContactLogger.onChange = function (kind, rec) { ... };
 * where kind is "editing"|"closed" and rec is
 *   { key, caseId, personId, recorderId, name, role }.
 * A handler that throws is swallowed and cannot affect CMS.
 *
 * NOTE for endpoint/locking use: a "closed" event is NOT guaranteed. If the tab
 * crashes, the network drops, or the browser is killed, no poll runs and nothing
 * fires. Do not treat "editing" as a lock that only "closed" releases — treat the
 * repeated polling as a heartbeat and give the server-side record a TTL, or you
 * will strand contacts in a permanently-"being edited" state.
 *
 * Mechanism: PURE OBSERVATION. A low-frequency poll locates the (same-origin)
 * ContactDetails frame and passively READS the CMS's own state:
 *   - win.bRHPOpen / win.sRHPType  -> is a victim/witness edit panel open?
 *   - win.objDataRow               -> the selected left-hand row (ContactType,
 *                                     rowIndex) used to compute the slot index.
 *   - hidden <input> values        -> ids/names at that index.
 * It NEVER wraps, patches or assigns to any CMS function or variable, so it
 * cannot change CMS behaviour.
 *
 * Cost of pure observation vs a function hook: a transition is reported up to one
 * poll interval late, and an open-then-close inside a single interval is not seen
 * at all (neither event fires, so the pair stays balanced).
 */
(function () {
  var INTERVAL = 3000; // ms between observation passes
  var MAXDEPTH = 64;
  var FRAGMENT = "uaccContactDetails.aspx";
  var VICWIT = "VW"; // VICTIMS_WITNESSES_CONTACT
  var timer;
  var lastRec = null; // last reported contact; null means nothing open
  var lastKey = ""; // its key; "" means nothing open

  function log(msg) {
    if (typeof console !== "undefined" && console.log) {
      console.log(msg);
    }
  }

  function trim(s) {
    return String(s == null ? "" : s).replace(/^\s+|\s+$/g, "");
  }

  // Resolve a hidden field by id, tolerating document-mode-5 quirks.
  function getEl(win, id) {
    var d = win.document;
    var el = null;
    if (d.getElementById) {
      el = d.getElementById(id);
    }
    if (!el && d.all) {
      el = d.all[id];
    }
    return el;
  }

  function fieldVal(win, id) {
    var el = getEl(win, id);
    return el ? el.value : "";
  }

  // Split a CSV hidden field the same way the page does, when we can.
  function splitCsv(win, s) {
    try {
      if (typeof win.trimAndSplit === "function") {
        return win.trimAndSplit(s);
      }
    } catch (e) {}
    return String(s == null ? "" : s).split(",");
  }

  // The contact-recorder id for a person: prefer the per-index witness column,
  // else look the person up in the "personId,version,recorderId|..." cache so it
  // resolves whichever role row (Victim or Witness) was clicked.
  function recorderIdFor(win, personId, idx) {
    var perIdx = splitCsv(
      win,
      fieldVal(win, "hidint64OldContactRecorderIdCSV"),
    );
    var v = perIdx[idx];
    if (v && v !== "0") {
      return v;
    }
    var cache = fieldVal(win, "hidContactRecorderWitnessCache");
    if (cache) {
      var rows = cache.split("|");
      var i;
      var cells;
      for (i = 0; i < rows.length; i++) {
        if (!rows[i]) {
          continue;
        }
        cells = rows[i].split(",");
        if (cells[0] === personId) {
          return cells[2];
        }
      }
    }
    return "";
  }

  // Reproduce customClickedRow's row->slot-index math from the selected row.
  // Pinned to CMS 24.0.01's victim/witness/alt-contact layout (3 slots each).
  function indexForRow(win, row) {
    if (!row) {
      return -1;
    }
    var types = splitCsv(win, fieldVal(win, "hidContactType"));
    var nDef = 0;
    var nVic = 0;
    var i;
    for (i = 0; i < types.length; i++) {
      if (types[i] === "Def") {
        nDef++;
      } else if (types[i] === "Victim") {
        nVic++;
      }
    }
    var r = row.rowIndex;
    if (row.ContactType === "Victim") {
      return (r - 1) * 3 + nDef;
    }
    return (nVic + r - 1) * 3 + nDef;
  }

  // Passively read the currently-open victim/witness contact from one frame.
  // Returns an object or null. Reads only.
  function readOpenContact(win) {
    if (!win.bRHPOpen || win.sRHPType !== VICWIT) {
      return null;
    }
    var row = win.objDataRow;
    var idx = indexForRow(win, row);
    if (idx < 0) {
      return null;
    }
    var ids = splitCsv(win, fieldVal(win, "hidContactId"));
    var personId = ids[idx];
    if (personId == null || personId === "" || personId === "0") {
      return null;
    }
    var first = splitCsv(win, fieldVal(win, "hidContactFirstName"));
    var sur = splitCsv(win, fieldVal(win, "hidContactSurname"));
    var recorderId = recorderIdFor(win, personId, idx);
    var role = row && row.ContactType ? row.ContactType : "";
    // i32CaseId is a global on the ContactDetails page; any endpoint will want it.
    var caseId = "";
    try {
      caseId = win.i32CaseId ? String(win.i32CaseId) : "";
    } catch (e) {}
    var rec = {};
    rec.key = caseId + "/" + personId + "/" + (recorderId || "-") + "/" + role;
    rec.caseId = caseId;
    rec.personId = personId;
    rec.recorderId = recorderId;
    rec.name = trim((first[idx] || "") + " " + (sur[idx] || ""));
    rec.role = role;
    return rec;
  }

  // Walk every nested frame; return the open contact from the first same-origin
  // ContactDetails frame that has one, or null.
  function findOpenContact(win, depth) {
    if (depth > MAXDEPTH) {
      return null;
    }
    var frames = win.frames;
    var i;
    var child;
    var href;
    var rec;
    for (i = 0; i < frames.length; i++) {
      child = frames[i];
      href = "";
      try {
        href = child.location.href;
      } catch (e) {} // same-origin only
      if (href.indexOf(FRAGMENT) !== -1) {
        rec = null;
        try {
          rec = readOpenContact(child);
        } catch (e) {}
        if (rec) {
          return rec;
        }
      }
      rec = findOpenContact(child, depth + 1);
      if (rec) {
        return rec;
      }
    }
    return null;
  }

  /* ===================================================================
   * OUTPUT SINK — THE ONE PLACE TO CHANGE TO CALL AN ENDPOINT
   * -------------------------------------------------------------------
   * sendEvent() is the single funnel for every event this script produces.
   * It is called exactly twice in the code below (once for "editing", once
   * for "closed"), so changing it changes all reporting.
   *
   * TO START CALLING AN ENDPOINT:
   *   1. set ENDPOINT_URL below (keep it a SAME-ORIGIN relative path)
   *   2. uncomment the postEvent(kind, rec) line inside sendEvent()
   * Nothing else in this file needs to change. postEvent() is written and
   * ready — it just isn't called.
   *
   * Constraints that shaped the helpers (IE mode / document-mode 5):
   *   - No JSON object, so the body is form-encoded by hand, not stringified.
   *   - XMLHttpRequest may be absent; ActiveXObject is the fallback.
   *   - Keep ENDPOINT_URL same-origin. Old IE needs XDomainRequest for
   *     cross-origin and cannot set request headers on it, which would break
   *     the Authorization header below.
   *   - The "closed" fired from shutdown() happens during unload, where an
   *     async request may be cancelled by the browser (there is no
   *     sendBeacon here). Treat "closed" as best-effort and rely on a
   *     server-side TTL — see the note in the file header.
   * =================================================================== */

  var ENDPOINT_URL = ""; // e.g. "/global-components/case-locking/api/cms-contact-view"
  var ID_TOKEN_STORAGE_KEY = "cms-auth-id-token"; // written to top-window localStorage by the auth flow

  // THE SINK. kind is "editing" | "closed"; rec is
  // { key, caseId, personId, recorderId, name, role }.
  function sendEvent(kind, rec) {
    log("[cc] " + kind + " " + describe(rec));

    // Section presence — isolated so it can never affect CMS. Routed to the
    // selected transport (PRESENCE_METHOD); both share this detection + the banner.
    try {
      if (kind === "editing") {
        presenceBegin(rec);
      } else if (kind === "closed") {
        presenceEnd();
      }
    } catch (e) {}

    // postEvent(kind, rec);   // <-- UNCOMMENT to POST (set ENDPOINT_URL first)

    // Runtime seam: lets a consumer hook in without editing this file.
    // Fully isolated — anything it throws is swallowed and cannot affect CMS.
    var api = window.__ccContactLogger;
    if (api && typeof api.onChange === "function") {
      try {
        api.onChange(kind, rec);
      } catch (e) {}
    }
  }

  function describe(rec) {
    var msg = "caseId=" + (rec.caseId || "-");
    msg = msg + " personId=" + rec.personId;
    msg = msg + " contactRecorderId=" + (rec.recorderId || "-");
    msg = msg + ' name="' + rec.name + '"';
    if (rec.role) {
      msg = msg + " role=" + rec.role;
    }
    return msg;
  }

  /* ===================================================================
   * SECTION PRESENCE (expansion of concern 1) — relay-driven.
   * -------------------------------------------------------------------
   * The IE Internet zone forbids the CMS page making the cross-origin presence
   * calls directly (prompt). So on "editing" we hand the sectionId to the polaris
   * presence relay via a Domain=cps.gov.uk cookie; the relay (polaris origin) does
   * the same-origin POST/heartbeat/poll and writes the member list back to a result
   * cookie, which we poll. When more than one user is in the section we show a
   * banner at the top of the edit frame. On "closed" we clear the command cookie
   * (relay stops) and remove the banner. See cms-presence-relay.html and memory
   * reference_cms_polaris_xorigin_zone.
   * =================================================================== */

  // Relay served DIRECTLY by nginx from the cms-augmentation feature folder (was the blob
  // route /global-components/<env>/cms-presence-relay.html). Same-origin relative path — this
  // client runs inside the proxied CMS shell, so it resolves to this proxy origin (removes the
  // reference's hard-coded QA host / env-specific TODO). See features/cms-augmentation.conf.
  var PRESENCE_RELAY_URL = "/presence-relay.html";
  var PRESENCE_CMD_COOKIE = "cc_presence_cmd";
  var PRESENCE_RESULT_COOKIE = "cc_presence_result";
  var PRESENCE_COOKIE_DOMAIN = "cps.gov.uk";
  var PRESENCE_SECTION_KIND = "VICTIM_WITNESS";
  var PRESENCE_POLL_MS = 2000;
  var PRESENCE_BANNER_ID = "ccPresenceBanner";
  // Show the banner when member count >= this. 1 = show even when it's only you
  // (handy for dev/visibility). Set to 2 for production: only alert when ANOTHER
  // user is also in the section (HEARTBEAT.md: "later this should be 2").
  var PRESENCE_BANNER_MIN = 1;

  // Which presence TRANSPORT is active. Two parallel implementations that share the
  // detection (this concern's editing/closed transitions) and the banner; only the
  // wire mechanism differs. Flip this one line to switch:
  //   "relay" - hidden polaris-origin iframe + cps.gov.uk cookie bridge (XHR).
  //   "jsonp" - <script src> JSONP via the presence-jsonp adapter (no iframe, no
  //             cookie bridge; DELETEs the session on leave). See presenceJsonp*.
  var PRESENCE_METHOD = "jsonp";

  var presenceRelayFrame = null; // hidden relay iframe (spawned once)
  var presenceActiveSid = ""; // section we're currently registering
  var presencePollTimer = null; // result-cookie poll

  function presenceWriteCookie(name, val) {
    try {
      document.cookie =
        name +
        "=" +
        encodeURIComponent(val) +
        "; Domain=" +
        PRESENCE_COOKIE_DOMAIN +
        "; Path=/";
    } catch (e) {}
  }
  function presenceReadCookie(name) {
    var jar = "";
    try {
      jar = document.cookie || "";
    } catch (e) {
      return "";
    }
    var parts = jar.split(";"),
      i,
      s;
    for (i = 0; i < parts.length; i++) {
      s = parts[i];
      while (s.charAt(0) === " ") {
        s = s.substring(1);
      }
      if (s.substring(0, name.length + 1) === name + "=") {
        var raw = s.substring(name.length + 1);
        try {
          return decodeURIComponent(raw);
        } catch (e2) {
          return raw;
        }
      }
    }
    return "";
  }

  function ensureRelayFrame() {
    if (presenceRelayFrame) {
      return;
    }
    try {
      var f = document.createElement("iframe");
      f.src = PRESENCE_RELAY_URL;
      f.style.display = "none";
      document.documentElement.appendChild(f);
      presenceRelayFrame = f;
      log("[cc] presence relay iframe spawned");
    } catch (e) {}
  }

  // The victim/witness edit frame (same one the logger reads), for the banner.
  function findContactFrameWin(win, depth) {
    if (depth > MAXDEPTH) {
      return null;
    }
    var frames = win.frames,
      i,
      child,
      href,
      found;
    for (i = 0; i < frames.length; i++) {
      child = frames[i];
      href = "";
      try {
        href = child.location.href;
      } catch (e) {}
      if (href.indexOf(FRAGMENT) !== -1) {
        return child;
      }
      found = findContactFrameWin(child, depth + 1);
      if (found) {
        return found;
      }
    }
    return null;
  }

  function presenceShowBanner(emails) {
    var win = findContactFrameWin(window, 0);
    if (!win) {
      return;
    }
    try {
      var doc = win.document;
      var b = doc.getElementById(PRESENCE_BANNER_ID);
      if (!b) {
        // Anchor INLINE, right after the "Show:" dropdown (cboNShow) in the RHP, so
        // the banner never adds height at the top of the frame and pushes the
        // OK/Cancel buttons out of reach. cboNShow exists whenever a vic/wit RHP is
        // open, which is exactly when presence is active.
        var anchor = doc.getElementById("cboNShow");
        if (!anchor || !anchor.parentNode) {
          return;
        }
        b = doc.createElement("span");
        b.id = PRESENCE_BANNER_ID;
        b.style.marginLeft = "10px";
        b.style.padding = "2px 8px";
        b.style.background = "#fff3cd";
        b.style.border = "1px solid #d39e00";
        b.style.color = "#664d03";
        b.style.font = "bold 11px Arial";
        b.style.whiteSpace = "nowrap";
        if (anchor.nextSibling) {
          anchor.parentNode.insertBefore(b, anchor.nextSibling);
        } else {
          anchor.parentNode.appendChild(b);
        }
      }
      b.innerHTML = "\u26A0 Also viewing: " + emails;
    } catch (e) {}
  }

  function presenceRemoveBanner() {
    var win = findContactFrameWin(window, 0);
    if (!win) {
      return;
    }
    try {
      var b = win.document.getElementById(PRESENCE_BANNER_ID);
      if (b && b.parentNode) {
        b.parentNode.removeChild(b);
      }
    } catch (e) {}
  }

  // Read the relay's result cookie "<sid>||<count>||<emails>" and show/hide banner.
  function presencePoll() {
    var r = presenceReadCookie(PRESENCE_RESULT_COOKIE);
    if (!r) {
      return;
    }
    var parts = r.split("||");
    if (parts.length < 3) {
      return;
    } // "relay-ready" / not a result yet
    if (parts[0] !== presenceActiveSid) {
      return;
    } // result for a different/old section
    var count = parseInt(parts[1], 10);
    if (!isNaN(count) && count >= PRESENCE_BANNER_MIN) {
      presenceShowBanner(parts[2]);
    } else {
      presenceRemoveBanner();
    }
  }

  function presenceStart(rec) {
    if (!rec.caseId || !rec.personId) {
      return;
    }
    var sid = rec.caseId + ":" + PRESENCE_SECTION_KIND + ":" + rec.personId;
    if (sid === presenceActiveSid) {
      return;
    } // same section (e.g. victim<->witness row of one person)
    ensureRelayFrame();
    presenceActiveSid = sid;
    presenceWriteCookie(PRESENCE_CMD_COOKIE, sid);
    presenceRemoveBanner();
    if (!presencePollTimer) {
      presencePollTimer = window.setInterval(presencePoll, PRESENCE_POLL_MS);
    }
    log("[cc] presence start " + sid);
  }

  function presenceStop() {
    if (!presenceActiveSid) {
      return;
    }
    presenceActiveSid = "";
    presenceWriteCookie(PRESENCE_CMD_COOKIE, "");
    if (presencePollTimer) {
      window.clearInterval(presencePollTimer);
      presencePollTimer = null;
    }
    presenceRemoveBanner();
    log("[cc] presence stop");
  }

  // ---- Transport dispatch: route editing/closed to the selected method --------
  function presenceBegin(rec) {
    if (PRESENCE_METHOD === "jsonp") {
      presenceJsonpStart(rec);
    } else {
      presenceStart(rec);
    }
  }
  function presenceEnd() {
    if (PRESENCE_METHOD === "jsonp") {
      presenceJsonpStop();
    } else {
      presenceStop();
    }
  }

  /* ---- JSONP transport (parallel to the relay above) ----------------------
   * Same start/stop contract as the relay transport; selected by PRESENCE_METHOD.
   * Uses <script src> (NOT gated by the IE cross-origin XHR zone), so there's no
   * relay iframe and no cookie bridge — the adapter (handlePresenceJsonp) turns each
   * GET into the backend's real REST call. The JSONP response executes as JS, so the
   * callback receives a REAL object/array — no JSON parsing needed here (which the
   * XHR relay could not do in document-mode 5). Shares the banner with the relay.
   * ----------------------------------------------------------------------- */
  var PRESENCE_JSONP_BASE = "/global-components/presence-jsonp"; // same-origin on the proxy
  var PRESENCE_JSONP_TICK_MS = 3000; // heartbeat + poll cadence
  var PRESENCE_JSONP_TIMEOUT_MS = 8000; // per-call watchdog (JSONP has no error event)

  var presenceJsonpSeq = 0; // unique callback + cache-bust counter
  var presenceJsonpSessionId = ""; // presence-API session id
  var presenceJsonpActiveSid = ""; // section we're holding
  var presenceJsonpHbTimer = null;

  // Core JSONP call. onData(obj) with the executed object/array, or null on failure.
  function presenceJsonp(op, params, onData) {
    presenceJsonpSeq = presenceJsonpSeq + 1;
    var cbName = "__ccpj_" + presenceJsonpSeq;
    var done = false;
    var script = null;
    var timer = null;

    function cleanup() {
      if (timer) {
        window.clearTimeout(timer);
        timer = null;
      }
      try {
        window[cbName] = undefined;
      } catch (e1) {}
      try {
        if (script && script.parentNode) {
          script.parentNode.removeChild(script);
        }
      } catch (e2) {}
    }

    window[cbName] = function (data) {
      if (done) {
        return;
      }
      done = true;
      cleanup();
      onData(data);
    };

    var url = PRESENCE_JSONP_BASE + "?op=" + encodeURIComponent(op);
    var k;
    for (k in params) {
      if (params.hasOwnProperty(k)) {
        url = url + "&" + k + "=" + encodeURIComponent(params[k]);
      }
    }
    url = url + "&callback=" + cbName + "&_=" + presenceJsonpSeq;

    timer = window.setTimeout(function () {
      if (done) {
        return;
      }
      done = true;
      cleanup();
      log("[cc] presence jsonp timeout op=" + op);
      onData(null);
    }, PRESENCE_JSONP_TIMEOUT_MS);

    try {
      script = document.createElement("script");
      script.type = "text/javascript";
      script.src = url;
      document.documentElement.appendChild(script);
    } catch (e) {
      if (!done) {
        done = true;
        cleanup();
        onData(null);
      }
    }
  }

  // Collect userEmail values ANYWHERE in the poll response, whatever its shape (a
  // bare array of members, or a wrapper object like {members:[...]}). The XHR relay
  // did this shape-agnostically with a regex over the raw text; document-mode 5 has
  // no JSON, so we walk the parsed object instead.
  function presenceCollectEmails(node, out, depth) {
    if (!node || depth > 6 || typeof node !== "object") {
      return;
    }
    var i, k;
    if (typeof node.length === "number") {
      for (i = 0; i < node.length; i++) {
        presenceCollectEmails(node[i], out, depth + 1);
      }
      return;
    }
    for (k in node) {
      if (node.hasOwnProperty(k)) {
        if (k === "userEmail" && node[k]) {
          out[out.length] = node[k];
        } else {
          presenceCollectEmails(node[k], out, depth + 1);
        }
      }
    }
  }
  function presenceJsonpEmails(data) {
    var out = [];
    presenceCollectEmails(data, out, 0);
    return out;
  }

  // Compact shape hint for logging: "array[N]" or "object{key,key}".
  function presenceJsonpDescribe(data) {
    if (data === null || typeof data !== "object") {
      return String(data);
    }
    if (typeof data.length === "number") {
      return "array[" + data.length + "]";
    }
    var ks = [],
      k;
    for (k in data) {
      if (data.hasOwnProperty(k)) {
        ks[ks.length] = k;
      }
    }
    return "object{" + ks.join(",") + "}";
  }

  function presenceJsonpTick() {
    if (!presenceJsonpSessionId) {
      return;
    }
    // Heartbeat (PUT-mapped); response ignored.
    presenceJsonp("heartbeat", { sid: presenceJsonpSessionId }, function () {});
    // Poll (GET-mapped) -> members -> banner.
    presenceJsonp("poll", { sid: presenceJsonpSessionId }, function (data) {
      if (data === null) {
        log("[cc] presence jsonp poll: no response (timeout)");
        return;
      }
      if (data.jsonpError) {
        log("[cc] presence jsonp poll error: " + data.jsonpError);
        return;
      }
      // An empty array = "no change since last poll" (backend delta protocol) — KEEP
      // the last banner rather than clearing it. Mirrors the relay's "[]" handling.
      if (typeof data.length === "number" && data.length === 0) {
        log("[cc] presence jsonp poll: no change");
        return;
      }
      var emails = presenceJsonpEmails(data);
      log(
        "[cc] presence jsonp poll: " +
          emails.length +
          " member(s) [" +
          emails.join(", ") +
          "] raw=" +
          presenceJsonpDescribe(data),
      );
      if (emails.length >= PRESENCE_BANNER_MIN) {
        presenceShowBanner(emails.join(", "));
      } else {
        presenceRemoveBanner();
      }
    });
  }

  function presenceJsonpStart(rec) {
    if (!rec.caseId || !rec.personId) {
      return;
    }
    var sid = rec.caseId + ":" + PRESENCE_SECTION_KIND + ":" + rec.personId;
    if (sid === presenceJsonpActiveSid) {
      return;
    } // same section (e.g. victim<->witness of one person)
    presenceJsonpStop(); // clears any prior session (and fires its DELETE)
    presenceJsonpActiveSid = sid;
    presenceRemoveBanner();
    log("[cc] presence jsonp create " + sid);
    presenceJsonp("create", { sectionId: sid }, function (data) {
      if (presenceJsonpActiveSid !== sid) {
        return;
      } // superseded while in flight
      if (data === null || data.jsonpError || !data.sessionId) {
        log("[cc] presence jsonp create failed for " + sid);
        return;
      }
      presenceJsonpSessionId = data.sessionId;
      log(
        "[cc] presence jsonp session " + presenceJsonpSessionId + " for " + sid,
      );
      presenceJsonpTick();
      presenceJsonpHbTimer = window.setInterval(
        presenceJsonpTick,
        PRESENCE_JSONP_TICK_MS,
      );
    });
  }

  function presenceJsonpStop() {
    if (presenceJsonpHbTimer) {
      window.clearInterval(presenceJsonpHbTimer);
      presenceJsonpHbTimer = null;
    }
    if (presenceJsonpSessionId) {
      log("[cc] presence jsonp delete " + presenceJsonpSessionId);
      // Best-effort DELETE on leave. NOT guaranteed on tab-close (a script injected
      // during unload may not run) — the server-side TTL stays the real backstop.
      presenceJsonp("remove", { sid: presenceJsonpSessionId }, function () {});
    }
    presenceJsonpSessionId = "";
    presenceJsonpActiveSid = "";
    presenceRemoveBanner();
  }

  // ---- HTTP helpers (ready to use; only called if you uncomment above) ----

  function newXhr() {
    try {
      if (typeof XMLHttpRequest !== "undefined") {
        return new XMLHttpRequest();
      }
    } catch (e) {}
    try {
      return new ActiveXObject("Microsoft.XMLHTTP");
    } catch (e) {}
    return null;
  }

  function readIdToken() {
    try {
      return window.localStorage.getItem(ID_TOKEN_STORAGE_KEY) || "";
    } catch (e) {
      return "";
    }
  }

  // Form-encoded by hand: there is no JSON object in document-mode 5.
  function encodeEvent(kind, rec) {
    var b = "event=" + encodeURIComponent(kind);
    b = b + "&caseId=" + encodeURIComponent(rec.caseId || "");
    b = b + "&personId=" + encodeURIComponent(rec.personId || "");
    b = b + "&contactRecorderId=" + encodeURIComponent(rec.recorderId || "");
    b = b + "&role=" + encodeURIComponent(rec.role || "");
    b = b + "&name=" + encodeURIComponent(rec.name || "");
    return b;
  }

  // Fire-and-forget POST. Never throws, never blocks, ignores the response.
  function postEvent(kind, rec) {
    if (!ENDPOINT_URL) {
      return;
    }
    var xhr = newXhr();
    if (!xhr) {
      return;
    }
    try {
      xhr.open("POST", ENDPOINT_URL, true); // async
      xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
      var token = readIdToken();
      if (token) {
        xhr.setRequestHeader("Authorization", "Bearer " + token);
      }
      xhr.send(encodeEvent(kind, rec));
    } catch (e) {}
  }

  function tick() {
    var rec = null;
    try {
      rec = findOpenContact(window, 0);
    } catch (e) {}
    var key = rec ? rec.key : "";
    if (key === lastKey) {
      return; // no change since last pass -> stay quiet
    }
    var prev = lastRec;
    lastRec = rec;
    lastKey = key;
    if (prev) {
      sendEvent("closed", prev); // includes the A -> B switch case
    }
    if (rec) {
      sendEvent("editing", rec);
    }
  }

  function start() {
    if (timer) {
      return;
    }
    tick();
    timer = window.setInterval(tick, INTERVAL);
  }

  function stop() {
    if (timer) {
      window.clearInterval(timer);
      timer = null;
    }
  }

  // Best-effort close when the shell goes away while a contact is still open.
  // Not guaranteed (a killed tab runs nothing) — see the TTL note in the header.
  function shutdown() {
    if (lastRec) {
      var prev = lastRec;
      lastRec = null;
      lastKey = "";
      try {
        sendEvent("closed", prev);
      } catch (e) {}
    }
    stop();
  }

  // Expose start/stop/tick so you can drive it from the console if needed, and
  // onChange as the seam for calling an endpoint.
  window.__ccContactLogger = { start: start, stop: stop, tick: tick };

  // Clean up the timer when the shell unloads.
  window.attachEvent("onunload", shutdown);

  start();
})();

/* ============================================================================
 * Concern 2: LOGIN -> AUTH IFRAME SPAWN
 * ----------------------------------------------------------------------------
 * Independent of the logger above. Watches the CMS login flow from the shell and,
 * when the user has just logged in, spawns the hidden /polaris auth iframe once.
 * That flow (the AD round-trip, a SERVER-SIDE part of this solution — NOT in this
 * file) runs on our own (polaris) origin and, in its final callback, stashes the
 * id-token in POLARIS localStorage. The presence relay (also polaris-origin) reads
 * it there same-origin — so there is NO cookie / cross-subdomain hand-off to the
 * CMS domain any more; this file just triggers the flow. See memory
 * reference_cms_polaris_xorigin_zone.
 *
 * Trigger: the shell's frameMain leaving uaulLogin.aspx (login -> app edge). It
 * re-fires if the site returns to login and leaves again. The shell boots ~seconds
 * BEFORE login completes, so we must wait for the edge, not fire on boot.
 *
 * IE MODE / DOCUMENT-MODE 5 — same constraints as concern 1 (no JSON, no arrow
 * functions, var + function declarations, no trailing commas).
 * ==========================================================================*/
(function () {
  var BUILD = "spawn1"; // bump on redeploy to confirm fresh bytes are live (cache!)
  var DEBUG = true; // verbose per-tick logging; window.__ccAuthHandover.setDebug(false) to quiet

  var POLARIS_PATH = "/polaris"; // auth entry (Polaris): /polaris -> /init -> /auth-refresh-inbound -> drop2 /init-entra (captures cookies + populates the Entra store + sets the id-token cookie).
  var LOGIN_FRAGMENT = "uaulLogin.aspx"; // frameMain is "on login" while its URL contains this
  var MAIN_FRAME = "frameMain"; // the shell frame login + app load into

  var WATCH_INTERVAL = 1000; // ms between login-state checks

  var wasOnLogin = false; // login-edge detector state
  var ticks = 0; // watch-loop counter (diagnostic)
  var watchTimer = null; // the poll interval; cleared after the first spawn (single-shot)

  function log(msg) {
    if (typeof console !== "undefined" && console.log) {
      console.log("[cc-auth] " + msg);
    }
  }
  function dlog(msg) {
    if (DEBUG) {
      log(msg);
    }
  }

  // Enumerate this window's direct child frames (name = url), tolerating
  // cross-origin children (the spawned auth iframe) which throw on access.
  function listFrames() {
    var out = "";
    try {
      var fr = window.frames;
      var i;
      var nm;
      var hrefx;
      for (i = 0; i < fr.length; i++) {
        nm = "#" + i;
        hrefx = "";
        try {
          nm = fr[i].name || "#" + i;
        } catch (e) {
          nm = "#" + i + "(name?)";
        }
        try {
          hrefx = fr[i].location.href || "";
        } catch (e2) {
          hrefx = "(x-origin)";
        }
        out = out + (out ? ", " : "") + nm + "=" + hrefx;
      }
    } catch (e) {
      return "(window.frames unreadable: " + e + ")";
    }
    return out || "(none)";
  }

  // The shell frame that login/app load into. Same-origin; guarded + logged.
  function mainFrameHref() {
    var f;
    try {
      f = window.frames[MAIN_FRAME];
    } catch (e) {
      dlog("frameMain lookup threw: " + e);
      return "";
    }
    if (!f) {
      dlog(
        "frameMain '" + MAIN_FRAME + "' not found. frames = " + listFrames(),
      );
      return "";
    }
    try {
      return f.location.href || "";
    } catch (e2) {
      dlog("frameMain.location unreadable (x-origin?): " + e2);
      return "";
    }
  }

  // Spawn the hidden auth iframe (fire-and-forget) and remove it once it settles.
  // The AD flow runs inside it and its callback stashes the id-token in polaris
  // localStorage; nothing to read back here.
  function spawnIframe() {
    try {
      var f = document.createElement("iframe");
      f.src = POLARIS_PATH;
      f.style.display = "none";
      f.onload = function () {
        try {
          if (f.parentNode) {
            f.parentNode.removeChild(f);
          }
        } catch (e) {}
      };
      document.documentElement.appendChild(f);
      log("spawned auth iframe src=" + POLARIS_PATH);
    } catch (e) {
      log("spawnIframe FAILED: " + e);
    }
  }

  // Fire on the login -> app edge: frameMain WAS on the login page and now isn't.
  function watch() {
    ticks = ticks + 1;
    var href = mainFrameHref();
    var onLogin = href ? href.indexOf(LOGIN_FRAGMENT) !== -1 : false;
    dlog(
      "tick " +
        ticks +
        ": frameMain=" +
        (href || "(empty)") +
        " onLogin=" +
        onLogin +
        " wasOnLogin=" +
        wasOnLogin,
    );
    if (!href) {
      return; // can't read frameMain this tick — keep wasOnLogin as-is
    }
    if (wasOnLogin && !onLogin) {
      log(
        "LOGIN EDGE — frameMain left " +
          LOGIN_FRAGMENT +
          " -> " +
          href +
          " — spawning auth iframe",
      );
      spawnIframe();
      // SINGLE-SHOT: stop polling after the first spawn — one auth capture per shell
      // (== per website) lifecycle.
      //
      // (a) This is possibly too simplistic. It does NOT handle re-authentication
      //     within the same shell (log out + back in won't re-spawn), and if a shell
      //     ever loads ALREADY authenticated (no login page shown) the edge never
      //     fires — nothing is captured and, since this clear never runs, the poll
      //     keeps going. Today's "full site reload on login" behaviour means fresh
      //     sessions always pass through the login page so the edge does fire; revisit
      //     if that ever changes.
      // (b) A cleaner design would hook the frameMain element's onload event (no
      //     polling at all) and spawn from there. Not done yet because the reliability
      //     of frame onload in this IE-mode frameset has NOT been proved — the poll is
      //     the known-good mechanism for now.
      if (watchTimer) {
        window.clearInterval(watchTimer);
        watchTimer = null;
      }
    }
    wasOnLogin = onLogin;
  }

  // On-demand state dump: window.__ccAuthHandover.debug()
  function debug() {
    log("=== debug snapshot [" + BUILD + "] ===");
    log("POLARIS_PATH=" + POLARIS_PATH + "  MAIN_FRAME=" + MAIN_FRAME);
    log("frameMain href = " + (mainFrameHref() || "(empty)"));
    log("child frames   = " + listFrames());
    log("wasOnLogin=" + wasOnLogin + "  ticks=" + ticks);
  }

  function setDebug(v) {
    DEBUG = !!v;
    log("DEBUG set to " + DEBUG);
  }

  // Console handles: force a spawn, dump state, or quiet the logging.
  window.__ccAuthHandover = {
    runNow: spawnIframe,
    debug: debug,
    setDebug: setDebug,
  };

  log(
    "auth-iframe watcher booted [" +
      BUILD +
      "] DEBUG=" +
      DEBUG +
      " — watching frame '" +
      MAIN_FRAME +
      "' every " +
      WATCH_INTERVAL +
      "ms; POLARIS_PATH=" +
      POLARIS_PATH,
  );
  watchTimer = window.setInterval(watch, WATCH_INTERVAL);
})();
