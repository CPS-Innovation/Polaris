// Feature 5 🌐 — CMS proxy response-body filters (js_body_filter handlers).
//
// The only feature-exclusive njs: it rewrites the CMS response body — swapping
// upstream domains/IPs back to the proxy host and injecting the Polaris/Materials
// launch buttons into the CMS menu bar. Per-environment app-setting reads come
// from the shared common.cms-detection module.
import common from "../common/cms-detection.js";
import ieMode from "../common/ie-mode.js";

// Getter factories, built on common.setting: a getter for a raw upstream value, and
// one that prefixes the protocol to build a proxy_pass target. `name` is the
// setting's env-var name. (Kept feature-local rather than central — see common/cms-detection.js.)
const upstream = (name) => (r) => common.setting(r, name);
const dest = (name) => (r) => process.env.ENDPOINT_HTTP_PROTOCOL + "://" + common.setting(r, name);

function replaceCmsDomains(r, data, flags) {
    __replaceCmsDomainsGeneric(r, data, flags, r.variables.host);
}

function replaceCmsDomainsAjaxViewer(r, data, flags) {
    __replaceCmsDomainsGeneric(r, data, flags, process.env.WEBSITE_HOSTNAME);
}

function cmsMenuBarFilters(r, data, flags) {
    data = __addAppLaunchButtonsToMenuBar(r, data, flags);
    replaceCmsDomains(r, data, flags);
}

function __addAppLaunchButtonsToMenuBar(r, data, flags) {
    data = data.replace(
        new RegExp('objMainWindow\\.top\\.frameData\\.objMasterWindow\\.top\\.frameServerJS\\.POLARIS_URL', 'g'),
        '"/polaris"'
    );
    data = data.replace(
        new RegExp('MENU_BAR_POLARIS_LOGO', 'g'),
        '"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAJKSURBVEhLtZXPS9tgGMe/6Q9r01Y7pyiWlWFRmTpXNybOy2AIO6gTBwpWYV4Ezz3sH1AP/gPuphdF1It49bLD8FAnMoa0MBXGJrYeiiFNbU2b+OTHac2yRNYPNHngffJ++zzfJ28YmUAVcej3qmFawfr6d8iyBIYBampcKJeBqaluiGIZbrdTzzLHVIBlP+LmpqjGLpcPXq8DR0dxrK5+xcDAQ4yNvVDXzDBtUTAYoGu9EqGhoQ7NzUESOMfgYAjb2yns75/h5CSt5v4Nyx4wjIxi0UFVsNjZSSIWe47Fxc9oa2tEMnmlZ1Vi2qLW1gVcXooUMQgEnJidjSKXKyGd5mktiJ6eehwe/sTGxgftAQMsV8DzBczPv6L+v8fISCdt/Fv999FohPz4pGdVYmtMeV4zfG7uJdrbH2Bv7weamnxYWnqL21saMQPu9R44nQxKJRkcJ9KUichkBEiScadtCfj9HvW+ubmNNs1hZqYb2WweKytfUFvrUtf+xKbJz5DPSzg9zaKjoxGRiB+pVAZrazHtAQMsV8CybuzunmF0tBPhcB36+0M4OEhjefkdEolfelYllgWUOj0eiXqex/j4E9r8HJOTT3FxwZHYIz2rEssCDB1IHCfQWD7G8fEVhobCmJ7uRV9fSM8wxrIHXi8Qj7+mt/cNBKEIn08z/F9YrkAZx4mJLjW2urmCrTEtFEp6ZB1bAsp3wS6mAtfXAl15/Zej09T4ODDD1OStrQQZqn3RJEnC8HAvWlr8+qo1TAX+B7Y8uA9VFgDuAGLN1z00rPbxAAAAAElFTkSuQmCC"'
    );
    // Add the Materials button
    data = data.replace(
        'var sMenuBarRight',
        'sMenuBarLeft += \'<td class="menu"><img alt="Launch Materials" border="0" class="clickable" onclick="openMaterials()" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAIAAABvFaqvAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAASBJREFUeNpidEjcykANwMRAJUA1g1gglIGGcH+ZOVy0c+6lHUefYNWAprKw6+SFG29xuig7UlNChBNTnIeLtTxJjwSvATU05xhjipcn62G1AF8YqcjxJfirIot4WMvYGIqTE9jx/qrAEIGwgQ4B+pf8WAOGCNCbQAbQpxAGgVhDBncefQLaD9EGZADD5e6jT0CfQmS/fPv94s13OBefi758/wOMfjgXGC7xSIFVO+UcUAGxXjty/uWa3Q8wxYGCkFRDQhhNXX4N6Ec0LwMFyQns2ilngSECDxogl8xYAwYqPLCmLr8O5JIWa2iBBcl0uLIesQZBMvDQLI8Gn0GMkMIfmLOQcxNaUsQsYeAZGKgSktZY4JpxpX2suZqGXgMIMACZaHaNNm2JEgAAAABJRU5ErkJggg=="></td>\';\n\tvar sMenuBarRight'
    );
    return data;
}

function __replaceCmsDomainsGeneric(r, data, flags, host) {
    // If a 302 has been issued then there's no point in processing in the response body
    if (r.status === 302) {
        r.sendBuffer(data, flags);
        return;
    }

    let replacements = [
        { old: common.setting(r, 'UPSTREAM_CMS_MODERN_DOMAIN_NAME'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_SERVICES_DOMAIN_NAME'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_DOMAIN_NAME'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_IP_CORSHAM'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_MODERN_IP_CORSHAM'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_IP_FARNBOROUGH'), new: host },
        { old: common.setting(r, 'UPSTREAM_CMS_MODERN_IP_FARNBOROUGH'), new: host }
    ];

    r.sendBuffer(__replaceContent(data, replacements), flags);
}

function __replaceContent(content, replacements) {

    for (let i = 0; i < replacements.length; i++) {
        let reg = /[-=./]/gm;
        let rep = replacements[i];
        let repold = (rep.old).replace(reg, "");
        let regexp = new RegExp(repold, 'g');
        content = content.replace(regexp, rep.new);
    }
    return content;
}

// ---------------------------------------------------------------------------
// Env-switch endpoints /cin2../cin5 (pre-prod only) — collapsed from four
// near-identical conf blocks into one handler. Sets the __CMSENV hint cookie for
// the chosen environment (cin3 IS "default" — see cms-detection.js / QUIRK D9),
// clears the OTHER three environments' BIG-IP pool + LB session cookies (they can
// carry the env token cms-detection scans for), then 302s to /CMS. The IE-desired
// gate reads $ieaction (features/common/ie-mode.js). As a js_content location it now
// inherits the server security headers on the 302 — harmless for a cookie-set +
// redirect; the old conf blocks dropped them (QUIRK B3). Pre-prod only.
const CIN_EXPIRE = "=deleted; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";

// Expired pool (ACP+AFP) then LB (C+F) cookies for every env EXCEPT `digit`, in the
// original conf order: all pool cookies (env-ascending, ACP before AFP), then all LB.
function _clearOtherCinEnvs(digit) {
    const pool = [];
    const lb = [];
    ["2", "3", "4", "5"].forEach((d) => {
        if (d === digit) return;
        const E = "CIN" + d;
        const e = "cin" + d;
        pool.push("BIGipServer~ent-s221~CPSACP-LTM-CM-WAN-" + E + "-" + e + ".cps.gov.uk_POOL" + CIN_EXPIRE);
        pool.push("BIGipServer~ent-s221~CPSAFP-LTM-CM-WAN-" + E + "-" + e + ".cps.gov.uk_POOL" + CIN_EXPIRE);
        lb.push("C-" + E + "-LBsessioncookie" + CIN_EXPIRE);
        lb.push("F-" + E + "-LBsessioncookie" + CIN_EXPIRE);
    });
    return pool.concat(lb);
}

function cinSwitch(r) {
    // IE-desired gate, rejecting a non-switchable non-IE browser — via the shared
    // helper (== the old inline nonie+nonconfigurable->402 / nonie+configurable->302).
    if (ieMode.coerce(r, "ie", true)) return;
    // proceed — switch environment (env picked from the request path).
    const m = (r.uri || "").match(/^\/cin([2-5])/);
    const digit = m ? m[1] : "";
    const env = digit === "3" ? "default" : "cin" + digit;
    r.headersOut["Set-Cookie"] = ["__CMSENV=" + env].concat(_clearOtherCinEnvs(digit));
    r.return(302, process.env.WEBSITE_SCHEME + "://" + r.variables.host + "/CMS");
}

export default {
    // CMS-upstream getters this feature js_sets (Corsham dests + the domain
    // getters). Built from this feature's own factories (over common.setting);
    // used by no other feature, so they live here rather than centrally.
    proxyDestinationCorsham: dest("UPSTREAM_CMS_IP_CORSHAM"),
    proxyDestinationModernCorsham: dest("UPSTREAM_CMS_MODERN_IP_CORSHAM"),
    upstreamCmsDomainName: upstream("UPSTREAM_CMS_DOMAIN_NAME"),
    upstreamCmsModernDomainName: upstream("UPSTREAM_CMS_MODERN_DOMAIN_NAME"),
    upstreamCmsServicesDomainName: upstream("UPSTREAM_CMS_SERVICES_DOMAIN_NAME"),
    // response-body filters
    replaceCmsDomains,
    replaceCmsDomainsAjaxViewer,
    cmsMenuBarFilters,
    // env-switch endpoints /cin2..5
    cinSwitch,
};
