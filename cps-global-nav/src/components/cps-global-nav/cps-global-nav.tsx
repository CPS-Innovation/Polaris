import { Component, Prop, h, Event, EventEmitter } from '@stencil/core';

@Component({
  tag: 'cps-global-nav',
  styleUrl: 'cps-global-nav.scss',
  shadow: true,
})
export class CpsGlobalNav {
  /**
   * The text to appear at the start of the second row
   */
  @Prop() name: string;

  @Event({
    eventName: 'globalNavLinkSelected',
    composed: true,
    cancelable: true,
    bubbles: true,
  })
  globalNavLinkSelected: EventEmitter<string>;

  connectedCallback() {
    console.log(window.location);
    window.addEventListener('popstate', function (event) {
      // Log the state data to the console
      console.log(event.state);
    });
  }

  private clickHandler = (ev: MouseEvent) => {
    const label = (ev.target as HTMLAnchorElement).innerText;
    console.log(label);
    this.globalNavLinkSelected.emit('foo');
  };

  render() {
    return (
      <div>
        <div class="level-1 background">
          <ul>
            <li>
              <a onClick={this.clickHandler}>Home</a>
            </li>
            <li>
              <a onClick={this.clickHandler}>Tasks</a>
            </li>
            <li>
              <a onClick={this.clickHandler}>Cases</a>
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
