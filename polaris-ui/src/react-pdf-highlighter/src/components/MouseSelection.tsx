import React, { Component } from "react";

import { asElement, isHTMLElement } from "../lib/pdfjs-dom";
import "../style/MouseSelection.css";

import type { LTWH } from "../types.js";

interface Coords {
  x: number;
  y: number;
}

interface State {
  locked: boolean;
  start: Coords | null;
  end: Coords | null;
}

interface Props {
  onSelection: (
    startTarget: HTMLElement,
    boundingRect: LTWH,
    resetSelection: () => void
  ) => void;
  onDragStart: () => void;
  onDragEnd: () => void;
  shouldStart: (event: MouseEvent) => boolean;
  onChange: (isVisible: boolean) => void;
}

class MouseSelection extends Component<Props, State> {
  state: State = {
    locked: false,
    start: null,
    end: null,
  };

  root?: HTMLElement;

  reset = () => {
    const { onDragEnd } = this.props;

    onDragEnd();
    this.setState({ start: null, end: null, locked: false });
  };

  getBoundingRect(start: Coords, end: Coords): LTWH {
    return {
      left: Math.min(end.x, start.x),
      top: Math.min(end.y, start.y),

      width: Math.abs(end.x - start.x),
      height: Math.abs(end.y - start.y),
    };
  }

  componentDidUpdate() {
    const { onChange } = this.props;
    const { start, end } = this.state;

    const isVisible = Boolean(start && end);

    onChange(isVisible);
  }

  componentDidMount() {
    if (!this.root) {
      return;
    }

    const that = this;

    const { onSelection, onDragStart, onDragEnd, shouldStart } = this.props;

    const container = asElement(this.root.parentElement);

    if (!isHTMLElement(container)) {
      return;
    }

    // Note: a CPS customisation here.  By caching containerBoundingRect the library
    //  makes the assumption that the pdf viewing control is always rooted at the same spot
    //  in the window.  In our use case, the page can scroll up and down and hence move
    //  the pdf control.  So we fix the bug by always calculating containerBoundingRect
    //  on demand and removing the use of the caching variable.

    //let containerBoundingRect: DOMRect | null = null;

    const containerCoords = (pageX: number, pageY: number) => {
      //if (!containerBoundingRect) {
      const containerBoundingRect = container.getBoundingClientRect();
      //}

      return {
        x: pageX - containerBoundingRect.left + container.scrollLeft,
        y:
          pageY -
          containerBoundingRect.top +
          container.scrollTop -
          window.scrollY,
      };
    };

    container.addEventListener("mousemove", (event: MouseEvent) => {
      const { start, locked } = this.state;

      if (!start || locked) {
        return;
      }

      that.setState({
        ...this.state,
        end: containerCoords(event.pageX, event.pageY),
      });
    });

    container.addEventListener("mousedown", (event: MouseEvent) => {
      //do not reset if the user is clicking on the redaction modal
      if (
        asElement(event.target).id !== "btn-redact" &&
        asElement(event.target).closest(".PdfHighlighter__tip-container")
      ) {
        return;
      }

      if (!shouldStart(event)) {
        this.reset();
        return;
      }

      const startTarget = asElement(event.target);
      if (!isHTMLElement(startTarget)) {
        return;
      }

      onDragStart();

      this.setState({
        start: containerCoords(event.pageX, event.pageY),
        end: null,
        locked: false,
      });

      const onMouseUp = (event: MouseEvent): void => {
        // emulate listen once
        event.currentTarget?.removeEventListener(
          "mouseup",
          onMouseUp as EventListener
        );

        const { start } = this.state;

        if (!start) {
          return;
        }

        const end = containerCoords(event.pageX, event.pageY);

        const boundingRect = that.getBoundingRect(start, end);

        if (
          !isHTMLElement(event.target) ||
          !container.contains(asElement(event.target)) ||
          !that.shouldRender(boundingRect)
        ) {
          that.reset();
          return;
        }

        that.setState(
          {
            end,
            locked: true,
          },
          () => {
            const { start, end } = that.state;

            if (!start || !end) {
              return;
            }

            if (isHTMLElement(event.target)) {
              onSelection(startTarget, boundingRect, that.reset);

              onDragEnd();
              window.getSelection()?.removeAllRanges();
            }
          }
        );
      };

      const { ownerDocument: doc } = container;
      if (doc.body) {
        doc.body.addEventListener("mouseup", onMouseUp);
      }
    });
  }

  shouldRender(boundingRect: LTWH) {
    return boundingRect.width >= 1 && boundingRect.height >= 1;
  }

  render() {
    const { start, end } = this.state;

    return (
      <div
        className="MouseSelection-container"
        ref={(node) => {
          if (!node) {
            return;
          }
          this.root = node;
        }}
      >
        {start && end ? (
          <div
            className="MouseSelection"
            style={this.getBoundingRect(start, end)}
          />
        ) : null}
      </div>
    );
  }
}

export default MouseSelection;
