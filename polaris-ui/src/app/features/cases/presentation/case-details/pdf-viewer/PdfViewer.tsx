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
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
import { NewPdfHighlight } from "../../../domain/NewPdfHighlight";
import { Footer } from "./Footer";
import { PdfHighlight } from "./PdfHighlifght";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { useControlledRedactionFocus } from "../../../../../common/hooks/useControlledRedactionFocus";
import { sortRedactionHighlights } from "../utils/sortRedactionHighlights";
import { IS_REDACTION_SERVICE_OFFLINE } from "../../../../../config";
import { LoaderUpdate } from "../../../../../common/presentation/components";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { UnsavedRedactionModal } from "../../../../../features/cases/presentation/case-details/modals/UnsavedRedactionModal";
import { roundToFixedDecimalPlaces } from "../../../../cases/hooks/utils/redactionUtils";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
const SCROLL_TO_OFFSET = 120;

type Props = {
  url: string;
  tabIndex: number;
  activeTabId: string | undefined;
  tabId: string;
  redactionTypesData: RedactionTypeData[];
  contextData: {
    documentType: string;
    documentId: string;
    saveStatus: SaveStatus;
    caseId: number;
  };
  headers: HeadersInit;
  documentWriteStatus: PresentationFlags["write"];
  searchHighlights: undefined | IPdfHighlight[];
  searchPIIHighlights: ISearchPIIHighlight[];
  redactionHighlights: IPdfHighlight[];
  focussedHighlightIndex: number;
  isOkToSave: boolean;
  areaOnlyRedactionMode: boolean;
  handleAddRedaction: (newRedaction: NewPdfHighlight[]) => void;
  handleRemoveRedaction: (id: string) => void;
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
  handleIgnoreRedactionSuggestion: CaseDetailsState["handleIgnoreRedactionSuggestion"];
};

const ensureAllPdfInView = () =>
  window.scrollY < SCROLL_TO_OFFSET &&
  window.scrollTo({ top: SCROLL_TO_OFFSET });

export const PdfViewer: React.FC<Props> = ({
  url,
  redactionTypesData,
  tabIndex,
  activeTabId,
  tabId,
  headers,
  documentWriteStatus,
  contextData,
  searchHighlights = [],
  searchPIIHighlights,
  redactionHighlights,
  isOkToSave,
  areaOnlyRedactionMode,
  handleAddRedaction,
  handleRemoveRedaction,
  handleRemoveAllRedactions,
  handleSavedRedactions,
  focussedHighlightIndex,
  handleIgnoreRedactionSuggestion,
}) => {
  console.log("searchPIIHighlights>>>>>11111", searchPIIHighlights);
  const containerRef = useRef<HTMLDivElement>(null);
  const scrollToFnRef = useRef<(highlight: IHighlight) => void>();
  const trackEvent = useAppInsightsTrackEvent();
  useControlledRedactionFocus(tabId, activeTabId, tabIndex);

  const highlights = useMemo(
    () => [
      ...searchHighlights,
      ...sortRedactionHighlights(redactionHighlights),
      ...searchPIIHighlights,
    ],
    [searchHighlights, redactionHighlights, searchPIIHighlights]
  );

  useEffect(() => {
    scrollToFnRef.current &&
      searchHighlights.length &&
      // searchHighlights *not* highlights, as the reference to highlights
      //  changes every time we make a redaction. We are only bothered
      //  about focussing search highlights anyway, so this works all round.
      scrollToFnRef.current(searchHighlights[focussedHighlightIndex]);
  }, [searchHighlights, focussedHighlightIndex]);

  const getPIIHighlightsWithSameText = useCallback(
    (text: string = "") => {
      const sameTextHighlights = searchPIIHighlights.filter(
        (highlight) => highlight.textContent === text
      );

      return sameTextHighlights;
    },
    [searchPIIHighlights]
  );

  const addRedaction = useCallback(
    (
      position: ScaledPosition,
      content: { text?: string; image?: string },
      redactionType: RedactionTypeData,
      redactAll: boolean
    ) => {
      if (redactAll) {
        const sameTextHighlights = getPIIHighlightsWithSameText(content?.text);
        const scaleFactor =
          position.boundingRect.width /
          sameTextHighlights[0].position.boundingRect.width;
        const newRedactions = sameTextHighlights.reduce((acc, highlight) => {
          const highlightBoundingRect = highlight.position.boundingRect;
          const rects = highlight.position.rects;
          const scaledBoundingRect = {
            x1: roundToFixedDecimalPlaces(
              highlightBoundingRect.x1 * scaleFactor
            ),
            y1: roundToFixedDecimalPlaces(
              highlightBoundingRect.y1 * scaleFactor
            ),
            x2: roundToFixedDecimalPlaces(
              highlightBoundingRect.x2 * scaleFactor
            ),
            y2: roundToFixedDecimalPlaces(
              highlightBoundingRect.y2 * scaleFactor
            ),
            width: roundToFixedDecimalPlaces(
              highlightBoundingRect.width * scaleFactor
            ),
            height: roundToFixedDecimalPlaces(
              highlightBoundingRect.height * scaleFactor
            ),
            pageNumber: highlightBoundingRect?.pageNumber,
          };
          const scaledRects = rects.map((rect) => ({
            x1: roundToFixedDecimalPlaces(rect.x1 * scaleFactor),
            y1: roundToFixedDecimalPlaces(rect.y1 * scaleFactor),
            x2: roundToFixedDecimalPlaces(rect.x2 * scaleFactor),
            y2: roundToFixedDecimalPlaces(rect.y2 * scaleFactor),
            width: roundToFixedDecimalPlaces(rect.width * scaleFactor),
            height: roundToFixedDecimalPlaces(rect.height * scaleFactor),
            pageNumber: rect?.pageNumber,
          }));
          acc.push({
            type: "redaction",
            position: {
              ...position,
              boundingRect: scaledBoundingRect,
              rects: scaledRects,
            },
            textContent: content.text,
            highlightType: "linear",
            redactionType: redactionType,
          });
          return acc;
        }, [] as NewPdfHighlight[]);
        handleAddRedaction(newRedactions);
        return;
      }
      const newRedaction: NewPdfHighlight = {
        type: "redaction",
        position,
        textContent:
          content.text ??
          "This is an area redaction and redacted content is unavailable",
        highlightType: content.image ? "area" : "linear",
        redactionType: redactionType,
      };

      handleAddRedaction([newRedaction]);
      window.getSelection()?.removeAllRanges();
    },
    [handleAddRedaction, getPIIHighlightsWithSameText]
  );

  const removeRedaction = (id: string) => {
    trackEvent("Remove Redact Content", {
      documentType: contextData.documentType,
      documentId: contextData.documentId,
      redactionsCount: 1,
    });
    handleRemoveRedaction(id);
  };

  const enableAreaSelection = useCallback(
    (event) => {
      return areaOnlyRedactionMode
        ? true
        : (event.target as HTMLElement).className === "textLayer";
    },
    [areaOnlyRedactionMode]
  );

  const suggestedRedactions = useMemo(() => {
    return searchPIIHighlights.filter(
      (highlight) => highlight.redactionStatus === "redacted"
    );
  }, [searchPIIHighlights]);

  return (
    <>
      <div
        className={
          areaOnlyRedactionMode
            ? `${classes.pdfViewer} ${classes.areaOnlyRedaction}`
            : classes.pdfViewer
        }
        ref={containerRef}
        data-testid={`div-pdfviewer-${tabIndex}`}
      >
        {contextData.saveStatus === "saving" && (
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
                enableAreaSelection={enableAreaSelection}
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
                      searchPIIData={{
                        searchPIIOn: content.highlightType === "searchPII",
                        textContent: content.text ?? "",
                        count: content.text
                          ? getPIIHighlightsWithSameText(content.text)?.length
                          : 0,
                        piiCategory: getPIIHighlightsWithSameText(
                          content.text
                        )[0]?.piiCategory,
                      }}
                      redactionTypesData={redactionTypesData}
                      onConfirm={(
                        redactionType: RedactionTypeData,
                        actionType: "redact" | "ignore" | "ignoreAll"
                      ) => {
                        switch (actionType) {
                          case "redact": {
                            trackEvent("Redact Content", {
                              documentType: contextData.documentType,
                              documentId: contextData.documentId,
                            });

                            addRedaction(
                              position,
                              content,
                              redactionType,
                              false
                            );
                            hideTipAndSelection();
                            return;
                          }
                          case "ignore": {
                            handleIgnoreRedactionSuggestion(
                              contextData.documentId,
                              content.text!,
                              false,
                              content.highlightId!
                            );
                            hideTipAndSelection();
                            return;
                          }
                          case "ignoreAll": {
                            handleIgnoreRedactionSuggestion(
                              contextData.documentId,
                              content.text!,
                              true,
                              content.highlightId!
                            );
                            hideTipAndSelection();
                            return;
                          }
                        }
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
        {!!(redactionHighlights.length || suggestedRedactions.length) && (
          <Footer
            contextData={contextData}
            tabIndex={tabIndex}
            redactionHighlightsCount={redactionHighlights.length}
            suggestedRedactionsCount={suggestedRedactions.length}
            searchPIIHighlightsCount={searchPIIHighlights.length}
            isOkToSave={isOkToSave}
            handleRemoveAllRedactions={handleRemoveAllRedactions}
            handleSavedRedactions={handleSavedRedactions}
          />
        )}
        <UnsavedRedactionModal
          documentId={contextData.documentId}
          caseId={contextData.caseId}
          handleAddRedaction={handleAddRedaction}
        />
      </div>
    </>
  );
};
