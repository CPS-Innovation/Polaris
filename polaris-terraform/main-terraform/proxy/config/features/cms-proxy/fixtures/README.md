# Mock fixtures — the fixture seam

These are **synthesised minimal stand-ins** for real CMS responses. They exist so
the body-rewrite / injection paths can be integration-tested without access to a
real CMS.

They are deliberately small, and each one carries only the **tokens the proxy
config actually looks for**, so the tests assert the _transformation_ rather than
a whole realistic page:

| Fixture                      | Exercises                                                       | Tokens it must contain                                                                        |
| ---------------------------- | --------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `uainGeneratedScript.aspx.js` | `sub_filter` rewrite of the c-button URL + CMS domain → `$host` | `var CASEWORK_TOOLS_URL = 'https://polaris.cps.gov.uk/launch/cms';`, CMS domains, `https://`    |
| `uainMenuBar.js`             | `cmsenv.cmsMenuBarFilters` (P + Materials button injection)      | `…frameServerJS.POLARIS_URL`, `MENU_BAR_POLARIS_LOGO`, `var sMenuBarRight`                      |
| `uacdCDTabs.aspx.html`       | `polaris-script.js` injection + `cmsenv.replaceCmsDomains`       | `</html>`, CMS domains                                                                          |
| `cms-page.html`              | generic CMS classic/modern proxying + domain rewrites            | CMS domains, `https://`                                                                         |

## Replacing these with real captures

If a real CMS response becomes available, **drop it in over the file of the same
name** — no test changes needed, provided it still contains the tokens above.
That is the whole point of the seam: the assertions target the transformation
(e.g. "`/launch/cms` became `/launch/cms-proxy`", "the Materials button HTML was
inserted"), not the surrounding markup.
