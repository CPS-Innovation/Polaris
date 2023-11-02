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
import { PresentationFlags } from "../../../domain/gateway/PipelineDocument";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import { Footer } from "./Footer";
import { PdfHighlight } from "./PdfHighlifght";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { useControlledRedactionFocus } from "../../../../../common/hooks/useControlledRedactionFocus";
import { sortRedactionHighlights } from "../utils/sortRedactionHighlights";
import { IS_REDACTION_SERVICE_OFFLINE } from "../../../../../config";
import { LoaderUpdate } from "../../../../../common/presentation/components";

const SCROLL_TO_OFFSET = 120;

type Props = {
  url: string;
  tabIndex: number;
  activeTabId: string | undefined;
  tabId: string;
  contextData: {
    documentType: string;
    documentId: string;
    isSaving: boolean;
  };
  headers: HeadersInit;
  documentWriteStatus: PresentationFlags["write"];
  searchHighlights: undefined | IPdfHighlight[];
  redactionHighlights: IPdfHighlight[];
  focussedHighlightIndex: number;
  isOkToSave: boolean;
  handleAddRedaction: (newRedaction: NewPdfHighlight) => void;
  handleRemoveRedaction: (id: string) => void;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
  handleReviewRedactions: (value: boolean) => void;
};

const ensureAllPdfInView = () =>
  window.scrollY < SCROLL_TO_OFFSET &&
  window.scrollTo({ top: SCROLL_TO_OFFSET });

export const PdfViewer: React.FC<Props> = ({
  url,
  tabIndex,
  activeTabId,
  tabId,
  headers,
  documentWriteStatus,
  contextData,
  searchHighlights = [],
  redactionHighlights,
  isOkToSave,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  handleReviewRedactions,
  focussedHighlightIndex,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const scrollToFnRef = useRef<(highlight: IHighlight) => void>();
  const trackEvent = useAppInsightsTrackEvent();
  useControlledRedactionFocus(tabId, activeTabId, tabIndex);

  const highlights = useMemo(
    () => [
      ...searchHighlights,
      ...sortRedactionHighlights(redactionHighlights),
    ],
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
    (position: ScaledPosition, content: { text?: string; image?: string }) => {
      const newRedaction: NewPdfHighlight = {
        type: "redaction",
        position,
        textContent:
          content.text ??
          "This is an area redaction and redacted content is unavailable",
        highlightType: content.image ? "area" : "linear",
      };

      handleAddRedaction(newRedaction);
    },
    [handleAddRedaction]
  );

  const removeRedaction = (id: string) => {
    trackEvent("Remove Redact Content", {
      documentType: contextData.documentType,
      documentId: contextData.documentId,
      redactionsCount: 1,
    });
    handleRemoveRedaction(id);
  };

  return (
    <>
      <div
        className={classes.pdfViewer}
        ref={containerRef}
        data-testid={`div-pdfviewer-${tabIndex}`}
      >
        {contextData.isSaving && (
          <div className={classes.spinner}>
            <Wait ariaLabel="Saving redaction, please wait" />
          </div>
        )}

        <PdfLoader
          url={url}
          headers={headers}
          beforeLoad={<Wait ariaLabel="Pdf loading, please wait" />}
          // To avoid reaching out to an internet-hosted asset we have taken a local copy
          //  of the library that PdfHighlighter links to and put that in our `public` folder.
          workerSrc={`${process.env.PUBLIC_URL}/pdf.worker.min.2.11.338.js`}
        >
          {(pdfDocument) => (
            <>
              <LoaderUpdate textContent="pdf loaded" />
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
                onSelectionFinished={(
                  position,
                  content,
                  hideTipAndSelection
                ) => {
                  // Danger: minification problem here (similar to PrivateBetaAuthorizationFilter)
                  //  `if(IS_REDACTION_SERVICE_OFFLINE)` just does not work in production. So work
                  //  by passing the original string around and comparing it here.
                  if (String(IS_REDACTION_SERVICE_OFFLINE) === "true") {
                    return (
                      <RedactionWarning
                        documentWriteStatus={"IsRedactionServiceOffline"}
                      />
                    );
                  }
                  if (documentWriteStatus !== "Ok") {
                    return (
                      <RedactionWarning
                        documentWriteStatus={documentWriteStatus}
                      />
                    );
                  }
                  return (
                    <RedactButton
                      onConfirm={() => {
                        trackEvent("Redact Content", {
                          documentType: contextData.documentType,
                          documentId: contextData.documentId,
                        });
                        addRedaction(position, content);
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
                      handleRemoveRedaction={removeRedaction}
                    />
                  );
                }}
                highlights={highlights}
              />
            </>
          )}
        </PdfLoader>
        {!!redactionHighlights.length && (
          <Footer
            contextData={contextData}
            tabIndex={tabIndex}
            redactionHighlights={redactionHighlights}
            isOkToSave={isOkToSave}
            handleRemoveAllRedactions={handleRemoveAllRedactions}
            handleSavedRedactions={handleSavedRedactions}
            handleReviewRedactions={handleReviewRedactions}
          />
        )}
      </div>
    </>
  );
};
