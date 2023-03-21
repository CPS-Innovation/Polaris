import React, { useCallback, useEffect, useMemo, useRef } from "react";

import {
  PdfLoader,
  PdfHighlighter,
  ScaledPosition,
  IHighlight,
} from "../../../../../../react-pdf-highlighter";

import classes from "./PdfViewer.module.scss";
import { Wait } from "./Wait";
import { RedactButton } from "./RedactButton";
import { RedactionWarning } from "./RedactionWarning";
import { PresentationFlags } from "../../../../../features/cases/domain/PipelineDocument";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import { Footer } from "./Footer";
import { PdfHighlight } from "./PdfHighlifght";

const SCROLL_TO_OFFSET = 120;

type Props = {
  url: string;
  headers: HeadersInit;
  redactStatus: PresentationFlags["write"];
  searchHighlights: undefined | IPdfHighlight[];
  redactionHighlights: IPdfHighlight[];
  focussedHighlightIndex: number;
  handleAddRedaction: (newRedaction: NewPdfHighlight) => void;
  handleRemoveRedaction: (id: string) => void;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
};

const ensureAllPdfInView = () =>
  window.scrollY < SCROLL_TO_OFFSET &&
  window.scrollTo({ top: SCROLL_TO_OFFSET });

export const PdfViewer: React.FC<Props> = ({
  url,
  headers,
  redactStatus,
  searchHighlights = [],
  redactionHighlights,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  focussedHighlightIndex,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const scrollToFnRef = useRef<(highlight: IHighlight) => void>();

  const highlights = useMemo(
    () => [...searchHighlights, ...redactionHighlights],
    [searchHighlights, redactionHighlights]
  );

  useEffect(() => {
    scrollToFnRef.current &&
      searchHighlights.length &&
      // searchHighlights *not* highlights, as the reference to highlights
      //  changes every time we make a redaction. We are only bothered
      //  about focussing search highlights anyway, so this works all round.
      scrollToFnRef.current(searchHighlights[focussedHighlightIndex]);
  }, [searchHighlights, focussedHighlightIndex]);

  const addRedaction = useCallback(
    (position: ScaledPosition, isAreaHighlight: boolean) => {
      const newRedaction: NewPdfHighlight = {
        type: "redaction",
        position,
        highlightType: isAreaHighlight ? "area" : "linear",
      };

      handleAddRedaction(newRedaction);
    },
    [handleAddRedaction]
  );

  return (
    <>
      <div
        className={classes.pdfViewer}
        ref={containerRef}
        data-testid="div-pdfviewer"
      >
        <PdfLoader url={url} headers={headers} beforeLoad={<Wait />}>
          {(pdfDocument) => (
            <PdfHighlighter
              onWheelDownwards={ensureAllPdfInView}
              pdfDocument={pdfDocument}
              enableAreaSelection={(event) =>
                (event.target as HTMLElement).className === "textLayer"
              }
              onScrollChange={() => {}}
              pdfScaleValue="page-width"
              scrollRef={(scrollTo) => {
                scrollToFnRef.current = scrollTo;
                // imperatively trigger as soon as we have reference to the scrollTo function
                if (highlights.length) {
                  scrollTo(highlights[0]);
                }
              }}
              onSelectionFinished={(position, content, hideTipAndSelection) => {
                if (redactStatus !== "Ok") {
                  return <RedactionWarning />;
                }
                return (
                  <RedactButton
                    onConfirm={() => {
                      addRedaction(position, !!content.image);
                      hideTipAndSelection();
                    }}
                  />
                );
              }}
              highlightTransform={(
                highlight,
                index,
                setTip,
                hideTip,
                _, // viewPortToScaled helper function
                __, // screenshot (an image if this is an area highlight)
                isScrolledTo
              ) => {
                return (
                  <PdfHighlight
                    highlight={highlight}
                    index={index}
                    setTip={setTip}
                    hideTip={hideTip}
                    isScrolledTo={isScrolledTo}
                    handleRemoveRedaction={handleRemoveRedaction}
                  />
                );
              }}
              highlights={highlights}
            />
          )}
        </PdfLoader>
        {!!redactionHighlights.length && (
          <Footer
            redactionHighlights={redactionHighlights}
            handleRemoveAllRedactions={handleRemoveAllRedactions}
            handleSavedRedactions={handleSavedRedactions}
          />
        )}
      </div>
    </>
  );
};
