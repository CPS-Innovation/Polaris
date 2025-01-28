import { Component, Prop, h, Event, EventEmitter, State } from "@stencil/core";
// import { getCaseDetails, lookupUrn } from "../../api/polaris-client";
// import { ensureMsalLoggedIn } from "../../ad-auth/ensure-msal-logged-in";
//import { formatLeadDefendantName } from "../../utils/format-lead-defendant-name";

//import { CaseDetails } from "../../domain/CaseDetails";
@Component({
  tag: "cps-global-nav-experimental",
  styleUrl: "../components/cps-global-nav/cps-global-nav.scss",
  shadow: true,
})
export class CpsGlobalNavExperimental {
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

  @State() leadDefendantPresentationName: string = "Please wait...";

  @State() leadDefendantClassName: string = "please-wait";

  //private getCaseDatePromise: Promise<CaseDetails | null>;

  // private async getCaseDate() {

  //   try {
  //     await ensureMsalLoggedIn();
  //     const { urnRoot } = await lookupUrn(this.caseId);
  //     return await getCaseDetails(urnRoot, this.caseId);
  //   } catch (err) {
  //     return null;
  //   }
  // }

  async componentWillLoad() {
    console.log(window.location.search);
    //this.getCaseDatePromise = this.getCaseDate();
  }

  async componentDidLoad() {
    // if (!this.caseId) {
    //   return;
    // }
    // const caseDetails = await this.getCaseDatePromise;
    // if (!caseDetails) {
    //   this.leadDefendantPresentationName = `Error: could not retrieve case ${this.caseId}`;
    //   return;
    // }
    // this.leadDefendantPresentationName = formatLeadDefendantName(caseDetails);
  }

  componentWillRender() {
    this.leadDefendantPresentationName = this.name;
    this.leadDefendantClassName = "";
  }

  emitWindowEvent() {
    this.cpsGlobalNavEvent.emit("foo");
  }

  render() {
    return (
      <div>
        <div class="level-1 background">
          <ul>
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
            <li class="selected">
              <a>Details</a>
            </li>
            <li class="selected dropdown active">
              <a>Materials</a>
              <ul>
                <li>
                  <a class="disabled">Case materials</a>
                </li>
                <li>
                  <a>Bulk UM classification</a>
                </li>
              </ul>
            </li>
            <li>
              <a>Review</a>
            </li>
          </ul>
          <div>
            <slot />
            <a href="#">CMS Classic</a>
          </div>
        </div>
        <div class="background-divider"></div>
      </div>
    );
  }
}
