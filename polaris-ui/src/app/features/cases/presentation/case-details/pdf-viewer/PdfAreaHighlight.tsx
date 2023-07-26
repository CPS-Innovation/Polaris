import React from "react";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import classes from "./PdfAreaHighlight.module.scss";

interface Props {
  highlight: T_ViewportHighlight<IPdfHighlight>;

  isScrolledTo: boolean;
}

export const PdfAreaHighlight: React.FC<Props> = ({
  highlight: {
    position: { boundingRect },
    id,
    redactionAddedOrder,
    textContent,
  },
  isScrolledTo,
  ...otherProps
}) => (
  <div
    data-testid={`div-highlight-${id}`}
    data-test-isfocussed={isScrolledTo}
    className={`highlight-layer-wrapper ${classes["AreaHighlight"]} ${
      isScrolledTo ? classes["AreaHighlight--scrolledTo"] : ""
    }`}
  >
    <button
      aria-describedby={`redacted-text-${redactionAddedOrder}`}
      data-redaction-added-order={redactionAddedOrder}
      className={classes["AreaHighlight__part"]}
      style={boundingRect}
      {...otherProps}
    ></button>

    <p hidden id={`redacted-text-${redactionAddedOrder}`}>
      {textContent}
    </p>
  </div>
);
