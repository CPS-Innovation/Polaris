// Synthesised stand-in for the CMS toolbar "uainMenuBar.js".
// See fixtures/README.md — replace with a real capture if one becomes available.
//
// Must carry the three tokens cmsenv.__addAppLaunchButtonsToMenuBar rewrites.
// They are NOT named in this comment on purpose: the proxy rewrites the whole
// body, comments included, which would confuse the assertions. See the code
// below and fixtures/README.md for the list.

function buildMenuBar() {
  var sMenuBarLeft = '<td class="menu">left</td>';

  var polarisUrl = objMainWindow.top.frameData.objMasterWindow.top.frameServerJS.POLARIS_URL;
  var polarisLogo = MENU_BAR_POLARIS_LOGO;

  sMenuBarLeft += '<td class="menu"><img alt="Launch Polaris" src="' + polarisLogo + '" onclick="window.open(' + polarisUrl + ')"></td>';

  var sMenuBarRight = '<td class="menu">right</td>';

  // A CMS domain reference, to exercise the domain rewriting:
  var cmsHome = 'https://cms.cps.gov.uk/CMS/Home';

  return sMenuBarLeft + sMenuBarRight + cmsHome;
}
