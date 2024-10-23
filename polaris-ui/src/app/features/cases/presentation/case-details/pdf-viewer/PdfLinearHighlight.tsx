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
  onClick?: () => {};
}

export const PdfLinearHighlight: React.FC<Props> = ({
  highlight: {
    position: { rects },
    id,
    textContent,
  },
  highlight,
  isScrolledTo,
  type,
  onClick,
}) => {
  const className = `${classes["Highlight"]} ${
    isScrolledTo ? classes["Highlight--scrolledTo"] : ""
  }`;
  let groupId;
  if (type === "searchPII") {
    groupId = (highlight as ISearchPIIHighlight).groupId;
  }

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
        highlight-groupid={groupId}
      >
        {rects.map((rect, index) =>
          type === "search" || type === "searchPII" ? (
            <PdfLinearHighlightPartSearch
              key={`${rect.left}-${rect.top}`}
              rect={rect}
            />
          ) : (
            <PdfLinearHighlightPartRedaction
              key={`${rect.left}-${rect.top}`}
              rect={rect}
              id={id}
              textContent={textContent}
              onClick={onClick}
            />
          )
        )}
      </div>
    </div>
  );
};
