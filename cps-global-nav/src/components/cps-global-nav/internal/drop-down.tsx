import { Component, Listen, Prop, State, h } from "@stencil/core";
import { LinkProps } from "./LinkProps";

@Component({
  tag: "drop-down",
  shadow: false,
})
export class DropDown {
  @Prop() label: string;
  @Prop() links: LinkProps[];
  @Prop() menuAlignment: "left" | "right" = "left";
  @State() opened: boolean;

  @Listen("click", { target: "document" })
  checkForClickOutside(ev: MouseEvent) {
    if (!ev.composedPath().includes(this.topLevelHyperlink)) {
      this.opened = false;
    }
  }

  private topLevelHyperlink: HTMLButtonElement;

  private handleLabelClick = () => {
    this.opened = !this.opened;
  };

  render() {
    return (
      <li class={`dropdown ${this.opened ? "active" : ""} ${this.links.some(link => link.selected) ? "selected" : ""}`}>
        <button class="linkButton" onClick={this.handleLabelClick} ref={el => (this.topLevelHyperlink = el as HTMLButtonElement)}>
          {this.label}
        </button>
        <ul class={this.menuAlignment == "right" ? "align-right" : ""}>
          {this.links.map(link => (
            <nav-link {...link}></nav-link>
          ))}
        </ul>
      </li>
    );
  }
}
