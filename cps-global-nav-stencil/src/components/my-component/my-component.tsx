import { Component, Prop, h } from '@stencil/core';
// import { format } from '../../utils/utils';

@Component({
  tag: 'my-component',
  styleUrl: 'my-component.scss',
  shadow: true,
})
export class MyComponent {
  /**
   * The text to appear at the start of the second row
   */
  @Prop() name: string;

  // private getText(): string {
  //   return format(this.first, this.middle, this.last);
  // }

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
          <div>Ccccc</div>
        </div>
        <div class="background-divider"></div>
      </div>
    );
  }
}

{
  /* <div>Hello, World! I'm {this.getText()}</div>; */
}
