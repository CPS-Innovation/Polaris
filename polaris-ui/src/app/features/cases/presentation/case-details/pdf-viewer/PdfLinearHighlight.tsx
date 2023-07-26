import React from "react";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";

import classes from "./PdfLinearHighlight.module.scss";
import { PdfLinearHighlightPartRedaction } from "./PdfLinearHighlightPartRedaction";
import { PdfLinearHighlightPartSearch } from "./PdfLinearHighlightPartSearch";

interface Props {
  type: "search" | "redaction";
  isScrolledTo: boolean;
  highlight: T_ViewportHighlight<IPdfHighlight>;
}

export const PdfLinearHighlight: React.FC<Props> = ({
  highlight: {
    position: { rects },
    id,
    redactionAddedOrder,
    textContent,
  },
  isScrolledTo,
  type,
}) => {
  const className = `${classes["Highlight"]} ${
    isScrolledTo ? classes["Highlight--scrolledTo"] : ""
  }`;

  return (
    <div
      className={`highlight-layer-wrapper ${className}`}
      data-testid={`div-highlight-${id}`}
      data-test-isfocussed={isScrolledTo}
    >
      <div className={classes["Highlight__parts"]}>
        {rects.map((rect, index) =>
          type === "search" ? (
            <PdfLinearHighlightPartSearch key={index} rect={rect} />
          ) : (
            <PdfLinearHighlightPartRedaction
              key={index}
              rect={rect}
              redactionAddedOrder={redactionAddedOrder}
              textContent={textContent}
            />
          )
        )}
      </div>
    </div>
  );
};
