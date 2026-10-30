// Synthesised stand-in for the CMS "uainGeneratedScript.aspx" c-button script.
// See fixtures/README.md — replace with a real capture if one becomes available.

var CASEWORK_TOOLS_URL = 'https://polaris.cps.gov.uk/launch/cms';

// The CMS Modern session id drop1 mints from this script (auth-handover.drop1's integration
// test reaches it via the loopback host). Not a domain, so the rewrite leaves it alone.
var SESS_MODERN_USER_SESSION_ID = '00000000-0000-0000-0000-000000000001';

// Domain references the proxy is expected to rewrite to $host:
var CMS_CLASSIC = 'https://cms.cps.gov.uk/CMS/Home';
var CMS_MODERN = 'https://cmsmodern.cps.gov.uk/CMSModern/Home';
var CMS_SERVICES = 'https://cms-services.cps.gov.uk/api/ping';

function uainGeneratedScript() {
  return { CASEWORK_TOOLS_URL: CASEWORK_TOOLS_URL };
}
