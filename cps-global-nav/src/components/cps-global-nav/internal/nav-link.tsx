import { Component, Prop, h, Event, EventEmitter } from "@stencil/core";

type LinkMode = "standard" | "new-tab" | "emit-event" | "disabled";

@Component({
  tag: "nav-link",
  shadow: false,
})
export class NavLink {
  @Prop() label: string;
  @Prop() href: string;
  @Prop() selected: boolean;
  @Prop() disabled: boolean;
  @Prop() openInNewTab?: boolean;

  @Event({
    eventName: "cps-global-nav-event",
    composed: true,
    cancelable: true,
    bubbles: true,
  })
  cpsGlobalNavEvent: EventEmitter<string>;

  emitEvent = () => {
    this.cpsGlobalNavEvent.emit(this.href);
  };

  launchNewTab = () => {
    window.open(this.href, "_blank", "noopener,noreferrer");
  };

  render() {
    const mode: LinkMode = this.disabled || !this.href ? "disabled" : this.openInNewTab ? "new-tab" : !this.href.startsWith("http") ? "emit-event" : "standard";

    const renderLink = () => {
      switch (mode) {
        case "disabled":
          return (
            <a class="govuk-link disabled" aria-disabled={true} href={this.href}>
              {this.label}
            </a>
          );
        case "new-tab":
          return (
            <button class="linkButton" onClick={this.launchNewTab}>
              {this.label}
            </button>
          );
        case "emit-event":
          return (
            <button class="linkButton" onClick={this.emitEvent}>
              {this.label}
            </button>
          );
        default:
          return (
            <a class="govuk-link" href={this.href}>
              {this.label}
            </a>
          );
      }
    };

    return <li class={this.selected ? "selected" : ""}>{renderLink()}</li>;
  }
}
