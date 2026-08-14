// CMS-upstream getters for the cms-augmentation uaglCMS route (js_import'd as `cmsAug`).
//
// A feature-local copy of the two getter factories over common.cms-detection — the
// established pattern in this config (cms-proxy.js and polaris-ddei.js each keep their own;
// these getters are deliberately NOT central, so each feature that needs them redefines the
// two one-liners). Only the four the uaglCMS route uses are exported here.
//
// This is an njs MODULE. It is NOT the browser client script — that is
// cms-augmentation.js, served statically (see cms-augmentation.conf), never js_import'd.
import common from "../common/cms-detection.js";

const upstream = (name) => (r) => common.setting(r, name);
const dest = (name) => (r) => process.env.ENDPOINT_HTTP_PROTOCOL + "://" + common.setting(r, name);

export default {
  proxyDestinationCorsham: dest("UPSTREAM_CMS_IP_CORSHAM"),
  upstreamCmsDomainName: upstream("UPSTREAM_CMS_DOMAIN_NAME"),
  upstreamCmsModernDomainName: upstream("UPSTREAM_CMS_MODERN_DOMAIN_NAME"),
  upstreamCmsServicesDomainName: upstream("UPSTREAM_CMS_SERVICES_DOMAIN_NAME"),
};
