import { Component, Prop, h } from "@stencil/core";

@Component({
  tag: "nav-link",
  shadow: false,
})
export class NavLink {
  @Prop() label: string;
  @Prop() href: string;
  @Prop() selected: boolean;

  render() {
    return (
      <li class={this.selected ? "selected" : ""}>
        <a class={`govuk-link ${this.selected ? "disabled" : ""}`} aria-disabled={this.selected} href={this.href}>
          {this.label}
        </a>
      </li>
    );
  }
}
