import { Component, Prop, h, Event, EventEmitter, State } from "@stencil/core";
import { getLocationConfig } from "../../context/get-location-config";
import { LinkCode, MatchedPathMatcher } from "../../context/LocationConfig";

@Component({
  tag: "cps-global-nav",
  styleUrl: "cps-global-nav.scss",
  shadow: true,
})
export class CpsGlobalNav {
  /**
   * The text to appear at the start of the second row
   */
  @Prop() name: string;

  @Prop() forceEnvironment: string;

  @Event({
    eventName: "cpsGlobalNavEvent",
    composed: true,
    cancelable: true,
    bubbles: true,
  })
  cpsGlobalNavEvent: EventEmitter<string>;

  @State() config: MatchedPathMatcher;

  async componentWillLoad() {
    this.config = getLocationConfig(window);
  }

  linkHelper = (code: LinkCode, label: string) => ({ label, href: this.config?.onwardLinks[code], selected: this.config?.matchedLinkCode === code });

  emitWindowEvent() {
    this.cpsGlobalNavEvent.emit("foo");
  }

  render() {
    return (
      <div>
        <div class="level-1 background">
          <ul>
            <nav-link {...this.linkHelper("tasks", "Tasks")}></nav-link>
            <nav-link {...this.linkHelper("cases", "Cases")}></nav-link>
          </ul>
        </div>
        <div class="background-divider"></div>

        <div class="level-2 background-left-only">
          <div>
            <span class="name">{this.name}</span>
          </div>
          <ul>
            <nav-link {...this.linkHelper("details", "Details")}></nav-link>
            <drop-down
              label="Materials"
              links={[this.linkHelper("case-materials", "Case Materials"), this.linkHelper("bulk-um-classification", "Bulk UM classification")]}
            ></drop-down>
            <nav-link {...this.linkHelper("review", "Review")}></nav-link>
          </ul>
          <div class="slot-container">
            <slot />
          </div>
          <ul>
            <drop-down
              label="CMS Classic"
              menuAlignment="right"
              links={[
                { label: "Task list", href: "/task-list" },
                { label: "Cases", href: "/cases" },
                { label: "Case review", href: "/case-review" },
                { label: "Case materials", href: "/case-materials" },
                { label: "Pre-charge triage", href: "/pre-charge-triage" },
              ]}
            ></drop-down>
          </ul>
        </div>
        <div class="background-divider"></div>
      </div>
    );
  }
}
