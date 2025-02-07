/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/** Cant do onFocus adn onBlur as the implementation depends on the mouse events **/
import React, { Component } from "react";

import MouseMonitor from "./MouseMonitor";

interface Props {
  onMouseOver: (content: JSX.Element) => void;
  popupContent: JSX.Element;
  onMouseOut: () => void;
  onClick: (content: JSX.Element) => void;
  children: JSX.Element;
}

interface State {
  mouseIn: boolean;
}

export class Popup extends Component<Props, State> {
  state: State = {
    mouseIn: false,
  };

  render() {
    const { onMouseOver, popupContent, onMouseOut, onClick } = this.props;
    const newProp = { onClick: () => onClick(popupContent) };

    return (
      <div
        onMouseOver={() => {
          this.setState({ mouseIn: true });

          onMouseOver(
            <MouseMonitor
              onMoveAway={() => {
                if (this.state.mouseIn) {
                  return;
                }

                onMouseOut();
              }}
              paddingX={60}
              paddingY={30}
              children={popupContent}
            />
          );
        }}
        onMouseOut={() => {
          this.setState({ mouseIn: false });
        }}
      >
        {/* {this.props.children} */}
        {React.Children.map(this.props.children, (child) => {
          // Clone each child and add new props to it
          return React.cloneElement(child, { ...newProp });
        })}
      </div>
    );
  }
}

export default Popup;
