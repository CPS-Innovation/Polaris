import { Component, Prop, h, State, Fragment } from "@stencil/core";
import { getLocationConfig } from "../../context/get-location-config";
import { LinkCode, MatchedPathMatcher } from "../../context/LocationConfig";

type LinkHelperArg = { code: LinkCode; label: string; children?: LinkCode[]; openInNewTab?: boolean };

const SHOULD_SHOW_NAME = false;
const SHOULD_SHOW_CMS_LINKS = false;
const SHOULD_SHOW_MATERIALS_MENU = false;
@Component({
  tag: "cps-global-nav",
  styleUrl: "cps-global-nav.scss",
  shadow: true,
})
export class CpsGlobalNav {
  /**
   * The text to appear at the start of the second row
   */
  @Prop() name: string = "Please wait...";
  @State() config: MatchedPathMatcher;

  async componentWillLoad() {
    this.config = getLocationConfig(window);
  }

  linkHelper = ({ code, label, children = [], openInNewTab }: LinkHelperArg) => ({
    label,
    href: this.config.matchedLinkCode === code ? this.config.href : this.config?.onwardLinks[code],
    selected: this.config?.matchedLinkCode === code || children.includes(this.config?.matchedLinkCode),
    openInNewTab,
  });

  render() {
    return (
      <div>
        <div class="level-1 background">
          <ul>
            <nav-link {...this.linkHelper({ code: "tasks", label: "Tasks" })}></nav-link>
            <nav-link {...this.linkHelper({ code: "cases", label: "Cases", children: ["details", "case-materials", "review"] })}></nav-link>
          </ul>
          <ul>
            <nav-link label="Give feedback" href="https://forms.office.com/e/Cxmsq5xTWx"></nav-link>
          </ul>
        </div>
        <div class="background-divider"></div>
        {this.config.showSecondRow && (
          <>
            <div class="level-2">
              {SHOULD_SHOW_NAME && (
                <div class="background-left-only">
                  <span class="name">{this.name}</span>
                </div>
              )}
              <ul>
                <nav-link {...this.linkHelper({ code: "details", label: "Details" })}></nav-link>

                {SHOULD_SHOW_MATERIALS_MENU ? (
                  <drop-down
                    label="Materials"
                    links={[
                      this.linkHelper({ code: "case-materials", label: "Case Materials" }),
                      this.linkHelper({ code: "bulk-um-classification", label: "Bulk UM classification" }),
                    ]}
                  ></drop-down>
                ) : (
                  <nav-link {...this.linkHelper({ code: "case-materials", label: "Materials" })}></nav-link>
                )}
                <nav-link {...this.linkHelper({ code: "review", label: "Review" })}></nav-link>
              </ul>
              <div class="slot-container">
                <slot />
              </div>
              <ul>
                {SHOULD_SHOW_CMS_LINKS && (
                  <drop-down
                    label="CMS Classic"
                    menuAlignment="right"
                    links={[this.linkHelper({ code: "cms-pre-charge-triage", label: "Pre-charge triage", openInNewTab: true })]}
                  ></drop-down>
                )}
              </ul>
            </div>
            <div class="background-divider"></div>
          </>
        )}
      </div>
    );
  }
}
