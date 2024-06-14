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
import {
  NewPdfHighlight,
  PIIRedactionStatus,
} from "../../../domain/NewPdfHighlight";
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
  isSearchPIIOn: boolean;
  activeSearchPIIHighlights: ISearchPIIHighlight[];
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
  isSearchPIIOn,
  activeSearchPIIHighlights,
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
  const containerRef = useRef<HTMLDivElement>(null);
  const scrollToFnRef = useRef<(highlight: IHighlight) => void>();
  const trackEvent = useAppInsightsTrackEvent();
  useControlledRedactionFocus(tabId, activeTabId, tabIndex);

  const highlights = useMemo(
    () => [
      ...searchHighlights,
      ...sortRedactionHighlights(redactionHighlights),
      ...activeSearchPIIHighlights,
    ],
    [searchHighlights, redactionHighlights, activeSearchPIIHighlights]
  );

  useEffect(() => {
    scrollToFnRef.current &&
      searchHighlights.length &&
      // searchHighlights *not* highlights, as the reference to highlights
      //  changes every time we make a redaction. We are only bothered
      //  about focussing search highlights anyway, so this works all round.
      scrollToFnRef.current(searchHighlights[focussedHighlightIndex]);
  }, [searchHighlights, focussedHighlightIndex]);

  const getPIISuggestionsWithSameText = useCallback(
    (redactionText: string = "") => {
      const redactionSuggestionWithSameText = activeSearchPIIHighlights.filter(
        (highlight) => highlight.textContent === redactionText
      );
      return redactionSuggestionWithSameText;
    },
    [activeSearchPIIHighlights]
  );

  const getSelectedPIIHighlight = useCallback(
    (highlightGroupId: string = "") => {
      return activeSearchPIIHighlights.find(
        (highlight) => highlight.groupId === highlightGroupId
      );
    },
    [activeSearchPIIHighlights]
  );

  const addRedaction = useCallback(
    (
      position: ScaledPosition,
      content: { text?: string; image?: string },
      redactionType: RedactionTypeData
    ) => {
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
    [handleAddRedaction]
  );

  const addSearchPIIRedaction = useCallback(
    (groupId: string, isAcceptedAll: boolean) => {
      const selectedHighlight = getSelectedPIIHighlight(groupId);
      if (!selectedHighlight) {
        return;
      }
      let newRedactions: NewPdfHighlight[] = [
        {
          type: "redaction",
          position: selectedHighlight.position,
          textContent: selectedHighlight.textContent,
          highlightType: "linear",
          redactionType: selectedHighlight.redactionType,
          searchPIIId: selectedHighlight.id,
        },
      ];
      if (isAcceptedAll) {
        const highlightsWithSameSuggestion = getPIISuggestionsWithSameText(
          selectedHighlight.textContent
        );
        newRedactions = highlightsWithSameSuggestion.map((highlight) => ({
          type: "redaction",
          position: highlight.position,
          textContent: highlight.textContent,
          highlightType: "linear",
          redactionType: highlight.redactionType,
          searchPIIId: highlight.id,
        }));
      }

      handleAddRedaction(newRedactions);
    },
    [handleAddRedaction, getPIISuggestionsWithSameText, getSelectedPIIHighlight]
  );

  const removeRedaction = (id: string) => {
    trackEvent("Remove Redact Content", {
      documentType: contextData.documentType,
      documentId: contextData.documentId,
      redactionsCount: 1,
    });
    handleRemoveRedaction(id);

    const selectedRedactionHighlight = redactionHighlights.find(
      (highlight) => highlight.id === id
    );
    if (selectedRedactionHighlight?.searchPIIId) {
      handleIgnoreRedactionSuggestion(
        contextData.documentId,
        "",
        "initial" as const,
        selectedRedactionHighlight.searchPIIId
      );
    }
  };

  const enableAreaSelection = useCallback(
    (event) => {
      return areaOnlyRedactionMode
        ? true
        : (event.target as HTMLElement).className === "textLayer";
    },
    [areaOnlyRedactionMode]
  );

  const getWrapperClassName = () => {
    let className = classes.pdfViewer;
    if (areaOnlyRedactionMode)
      className = `${className} ${classes.areaOnlyRedaction}`;
    if (isSearchPIIOn) className = `${className} ${classes.searchPiiOn}`;
    return className;
  };

  return (
    <>
      <div
        className={getWrapperClassName()}
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
                      searchPIIData={
                        content.highlightType === "searchPII"
                          ? {
                              textContent: content?.text ?? "",
                              count:
                                getPIISuggestionsWithSameText(content?.text)
                                  ?.length ?? 0,
                            }
                          : undefined
                      }
                      redactionTypesData={redactionTypesData}
                      onConfirm={(
                        redactionType: RedactionTypeData,
                        actionType: PIIRedactionStatus | "redact"
                      ) => {
                        switch (actionType) {
                          case "redact": {
                            trackEvent("Redact Content", {
                              documentType: contextData.documentType,
                              documentId: contextData.documentId,
                              redactionType: redactionType?.name,
                            });

                            addRedaction(
                              position,
                              {
                                text: content.text ?? "",
                                image: content?.image,
                              },
                              redactionType
                            );
                            hideTipAndSelection();
                            return;
                          }
                          case "ignored":
                          case "ignoredAll":
                          case "accepted":
                          case "acceptedAll": {
                            if (!content?.text || !content?.highlightGroupId) {
                              return;
                            }
                            handleIgnoreRedactionSuggestion(
                              contextData.documentId,
                              content.text,
                              actionType,
                              content.highlightGroupId
                            );

                            hideTipAndSelection();
                            if (
                              actionType === "accepted" ||
                              actionType === "acceptedAll"
                            ) {
                              addSearchPIIRedaction(
                                content.highlightGroupId,
                                actionType === "acceptedAll"
                              );
                            }
                            if (
                              actionType === "ignored" ||
                              actionType === "ignoredAll"
                            ) {
                              trackEvent("Ignore Redaction Suggestion", {
                                documentType: contextData.documentType,
                                documentId: contextData.documentId,
                                redactionType: getSelectedPIIHighlight(
                                  content.highlightGroupId
                                )?.redactionType?.name,
                                piiCategory: getSelectedPIIHighlight(
                                  content.highlightGroupId
                                )?.piiCategory,
                                ignoreType:
                                  actionType === "ignored" ? "once" : "all",
                                ignoreCount:
                                  actionType === "ignored"
                                    ? 1
                                    : getPIISuggestionsWithSameText(
                                        content.text
                                      )?.length,
                              });
                            }
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
        {redactionHighlights.length && (
          <Footer
            contextData={contextData}
            tabIndex={tabIndex}
            totalRedactionsCount={redactionHighlights.length}
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
