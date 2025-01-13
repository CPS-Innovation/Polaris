import { Component, Prop, h, Event, EventEmitter } from "@stencil/core";
import { lookupUrn } from "../../api/polaris-client";
import { ensureMsalLoggedIn } from "../../auth/ensure-msal-logged-in";
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

  async connectedCallback() {
    try {
      await ensureMsalLoggedIn();
      const res = await lookupUrn(2149310);
      console.log(res);
    } catch (err) {
      //console.error(err);
    }
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
              <a>Home</a>
            </li>
            <li>
              <a>Tasks</a>
            </li>
            <li>
              <a>Cases</a>
            </li>
          </ul>
        </div>
        <div class="background-divider"></div>
        <div class="level-2 background-left-only">
          <div>{this.name}</div>
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
          </ul>
          <div>
            <button onClick={() => this.emitWindowEvent()}>Fire an event</button>
            <slot />
          </div>
        </div>
        <div class="background-divider"></div>
      </div>
    );
  }
}

{
  /* <div>Hello, World! I'm {this.getText()}</div>; */
}
