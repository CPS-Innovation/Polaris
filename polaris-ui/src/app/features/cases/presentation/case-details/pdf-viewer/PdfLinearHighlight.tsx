import React from "react";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";

import classes from "./PdfLinearHighlight.module.scss";
import { PdfLinearHighlightPartRedaction } from "./PdfLinearHighlightPartRedaction";
import { PdfLinearHighlightPartSearch } from "./PdfLinearHighlightPartSearch";

interface Props {
  type: "search" | "redaction" | "searchPII";
  isScrolledTo: boolean;
  highlight: T_ViewportHighlight<IPdfHighlight | ISearchPIIHighlight>;
}

export const PdfLinearHighlight: React.FC<Props> = ({
  highlight: {
    position: { rects },
    id,
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
      <div
        className={classes["Highlight__parts"]}
        text-content={textContent}
        highlight-type={type}
      >
        {rects.map((rect, index) =>
          type === "search" || type === "searchPII" ? (
            <PdfLinearHighlightPartSearch key={index} rect={rect} />
          ) : (
            <PdfLinearHighlightPartRedaction
              key={index}
              rect={rect}
              id={id}
              textContent={textContent}
            />
          )
        )}
      </div>
    </div>
  );
};
