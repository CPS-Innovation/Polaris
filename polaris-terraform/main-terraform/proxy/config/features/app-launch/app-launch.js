// Ten near-identical handover redirects collapsed into one js_content handler.
// Each /launch/<key> 302s to https://<host>/polaris?r=<hop>, where <hop> is the
// OutSystems auth-handover URL. TARGETS maps each launch key to its CMS host and its
// environment tier (*_TIER below); `os` is the full OutSystems host (the QA tier moved to
// the oapps domain, RNAV-40 / #2217, so it is no longer always a .outsystemsenterprise.com
// subdomain); `polaris` holds just the distinctive subdomain and the builder appends
// .cps.gov.uk.
// The one env-driven host (DEFAULT_UPSTREAM_CMS_DOMAIN_NAME) is a fixed app setting,
// read once at module load — the container env is populated before nginx loads this.
//
// The URL is built structurally: the final casework landing URL is single-encoded
// as the inner r=, then the whole hop URL is encoded again as the outer r= — the
// same double-encoding the static strings carried. Behaviour-preserving, proven
// byte-for-byte by app-launch.unit.test.js + 01-app-launch.integration.test.js.
// One row per environment tier. A tier moving (e.g. RNAV-40 / #2217: the QA tier to the oapps
// OutSystems host) is a one-line change here. `os` is the full OutSystems host.
const PROD_TIER = {
  os: "cps.outsystemsenterprise.com",
  polaris: "polaris",
  stage: "prod",
};
const QA_TIER = {
  os: "oapps-qa-notprod.int.cps.gov.uk",
  polaris: "polaris-qa-notprod",
  stage: "test",
};
// cpt/cmo brought across from the live config (FCT2-18732): the UAT tier.
const UAT_TIER = {
  os: "cps-tst1.outsystemsenterprise.com",
  polaris: "polaris-uat-notprod",
  stage: "uat",
};

// A launch target: the CMS host it hands over from, plus its tier's fields. (Explicit copy, not
// object spread: njs rejects `...` — "Token ... not supported in this version".)
function target(host, tier) {
  return {
    host: host,
    os: tier.os,
    polaris: tier.polaris,
    stage: tier.stage,
  };
}

// Launch key -> target. cpt -> cmscpt host, cmo -> cmo host (live naming); the -proxy keys
// hand over from the proxied-CMS host.
const TARGETS = {
  cms: target(process.env.DEFAULT_UPSTREAM_CMS_DOMAIN_NAME, PROD_TIER),
  cin2: target("cin2.cps.gov.uk", QA_TIER),
  cin3: target("cin3.cps.gov.uk", QA_TIER),
  cin4: target("cin4.cps.gov.uk", QA_TIER),
  cin5: target("cin5.cps.gov.uk", QA_TIER),
  cpt: target("cmscpt.cps.gov.uk", UAT_TIER),
  cmo: target("cmo.cps.gov.uk", UAT_TIER),
  "cms-proxy": target("polaris.cps.gov.uk", PROD_TIER),
  "cin2-proxy": target("polaris-qa-notprod.cps.gov.uk", QA_TIER),
  "cin3-proxy": target("polaris-qa-notprod.cps.gov.uk", QA_TIER),
  "cin4-proxy": target("polaris-qa-notprod.cps.gov.uk", QA_TIER),
  "cin5-proxy": target("polaris-qa-notprod.cps.gov.uk", QA_TIER),
  "cpt-proxy": target("polaris-uat-notprod.cps.gov.uk", UAT_TIER),
  "cmo-proxy": target("polaris-uat-notprod.cps.gov.uk", UAT_TIER),
};

function launch(r) {
  const t = TARGETS[r.uri.substring("/launch/".length)];
  if (!t) {
    r.return(404, "unknown launch target");
    return;
  }

  const os = t.os;
  // Every tier lands on the same casework home page (FCT2-21520 moved prod off
  // casework_blocks/home, the last per-target difference).
  const finalUrl = `https://${os}/casework/home?IsFromCMS=True`;
  const hop =
    `https://${os}/Casework_Patterns/auth-handover.html` +
    `?src=https://${t.polaris}.cps.gov.uk/global-components/${t.stage}/auth-handover.js` +
    `&stage=os-cookie-return` +
    `&r=${encodeURIComponent(finalUrl)}`;

  r.return(302, `https://${t.host}/polaris?r=${encodeURIComponent(hop)}`);
}

export default { launch };
