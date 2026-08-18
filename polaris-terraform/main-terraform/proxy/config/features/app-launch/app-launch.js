// Ten near-identical handover redirects collapsed into one js_content handler.
// Each /launch/<key> 302s to https://<host>/polaris?r=<hop>, where <hop> is the
// OutSystems auth-handover URL. TARGETS is a flat manifest — one self-contained
// row per launch key; `os`/`polaris` hold just the distinctive subdomains and the
// builder appends the shared parent domains (.outsystemsenterprise.com / .cps.gov.uk).
// The one env-driven host (DEFAULT_UPSTREAM_CMS_DOMAIN_NAME) is a fixed app setting,
// read once at module load — the container env is populated before nginx loads this.
//
// The URL is built structurally: the final casework landing URL is single-encoded
// as the inner r=, then the whole hop URL is encoded again as the outer r= — the
// same double-encoding the static strings carried. Behaviour-preserving, proven
// byte-for-byte by app-launch.unit.test.js + 01-app-launch.integration.test.js.
const TARGETS = {
  cms: {
    host: process.env.DEFAULT_UPSTREAM_CMS_DOMAIN_NAME,
    os: "cps",
    polaris: "polaris",
    stage: "prod",
    casework: "casework_blocks/home",
  },
  cin2: {
    host: "cin2.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  cin3: {
    host: "cin3.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  cin4: {
    host: "cin4.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  cin5: {
    host: "cin5.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  // cpt/cmo brought across from the live config (FCT2-18732). UAT-tier: OutSystems cps-tst1,
  // polaris-uat-notprod, stage uat. cpt -> cmscpt host, cmo -> cmo host (live naming).
  cpt: {
    host: "cmscpt.cps.gov.uk",
    os: "cps-tst1",
    polaris: "polaris-uat-notprod",
    stage: "uat",
    casework: "casework/home",
  },
  cmo: {
    host: "cmo.cps.gov.uk",
    os: "cps-tst1",
    polaris: "polaris-uat-notprod",
    stage: "uat",
    casework: "casework/home",
  },
  "cms-proxy": {
    host: "polaris.cps.gov.uk",
    os: "cps",
    polaris: "polaris",
    stage: "prod",
    casework: "casework_blocks/home",
  },
  "cin2-proxy": {
    host: "polaris-qa-notprod.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  "cin3-proxy": {
    host: "polaris-qa-notprod.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  "cin4-proxy": {
    host: "polaris-qa-notprod.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  "cin5-proxy": {
    host: "polaris-qa-notprod.cps.gov.uk",
    os: "cps-tst",
    polaris: "polaris-qa-notprod",
    stage: "test",
    casework: "casework/home",
  },
  "cpt-proxy": {
    host: "polaris-uat-notprod.cps.gov.uk",
    os: "cps-tst1",
    polaris: "polaris-uat-notprod",
    stage: "uat",
    casework: "casework/home",
  },
  "cmo-proxy": {
    host: "polaris-uat-notprod.cps.gov.uk",
    os: "cps-tst1",
    polaris: "polaris-uat-notprod",
    stage: "uat",
    casework: "casework/home",
  },
};

function launch(r) {
  const t = TARGETS[r.uri.substring("/launch/".length)];
  if (!t) {
    r.return(404, "unknown launch target");
    return;
  }

  const os = `${t.os}.outsystemsenterprise.com`;
  const finalUrl = `https://${os}/${t.casework}?IsFromCMS=True`;
  const hop =
    `https://${os}/Casework_Patterns/auth-handover.html` +
    `?src=https://${t.polaris}.cps.gov.uk/global-components/${t.stage}/auth-handover.js` +
    `&stage=os-cookie-return` +
    `&r=${encodeURIComponent(finalUrl)}`;

  r.return(302, `https://${t.host}/polaris?r=${encodeURIComponent(hop)}`);
}

export default { launch };
