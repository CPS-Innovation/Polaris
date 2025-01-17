import { Component, Prop, h, Event, EventEmitter, State } from "@stencil/core";
import { getCaseDetails, lookupUrn } from "../../api/polaris-client";
import { ensureMsalLoggedIn } from "../../ad-auth/ensure-msal-logged-in";
import { formatLeadDefendantName } from "../../utils/format-lead-defendant-name";
import { getCaseId } from "../../context/get-case-id";
import { CaseDetails } from "../../domain/CaseDetails";
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

  @Event({
    eventName: "cpsGlobalNavEvent",
    composed: true,
    cancelable: true,
    bubbles: true,
  })
  cpsGlobalNavEvent: EventEmitter<string>;

  @Event({
    eventName: "change",
    composed: true,
    cancelable: true,
    bubbles: true,
  })
  change: EventEmitter<string>;

  @State() leadDefendantPresentationName: string = "Please wait...";

  @State() leadDefendantClassName: string = "please-wait";

  private caseId: number | null;
  private getCaseDatePromise: Promise<CaseDetails | null>;

  private async getCaseDate() {
    try {
      await ensureMsalLoggedIn();
      const { urnRoot } = await lookupUrn(this.caseId);
      return await getCaseDetails(urnRoot, this.caseId);
    } catch (err) {
      return null;
    }
  }

  async componentWillLoad() {
    this.caseId = getCaseId(window);
    if (!this.caseId) {
      this.leadDefendantPresentationName = "Error: no case id in address";
      return;
    }
    this.getCaseDatePromise = this.getCaseDate();
  }

  async componentDidLoad() {
    if (!this.caseId) {
      return;
    }

    const caseDetails = await this.getCaseDatePromise;
    if (!caseDetails) {
      this.leadDefendantPresentationName = `Error: could not retrieve case ${this.caseId}`;
      return;
    }

    this.leadDefendantPresentationName = formatLeadDefendantName(caseDetails);
    this.leadDefendantClassName = "";
  }

  emitWindowEvent() {
    this.cpsGlobalNavEvent.emit("foo");
    this.change.emit("1");
  }

  render() {
    return (
      <div>
        <div class="level-1 background">
          <ul>
            <li>
              <a>Home</a>
            </li>
            <li>
              <a>Tasks</a>
            </li>
            <li>
              <a>Cases</a>
            </li>
          </ul>
          <div>
            <button onClick={() => this.emitWindowEvent()}>Fire an event</button>
          </div>
        </div>
        <div class="background-divider"></div>
        <div class="level-2 background-left-only">
          <div>
            <span class="name">{this.leadDefendantPresentationName}</span>
          </div>
          <ul>
            <li>
              <a>Overview</a>
            </li>
            <li>
              <a>Materials</a>
            </li>
            <li>
              <a>Review</a>
            </li>
            <li>
              <a>Triage</a>
            </li>
            <li>
              <a href="https://cps-dev.outsystemsenterprise.com/MikeTest/Home?Index=1">Page 1</a>
            </li>
            <li>
              <a href="https://cps-dev.outsystemsenterprise.com/MikeTest/Home?Index=2">Page 2</a>
            </li>
          </ul>
          <div>
            <slot />
          </div>
        </div>
        <div class="background-divider"></div>
      </div>
    );
  }
}
