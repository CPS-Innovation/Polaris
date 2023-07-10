import { useEffect, useCallback, useRef } from "react";

export const useControlledRedactionFocus = (
  tabId: string,
  activeTabId: string | undefined,
  tabIndex: number
) => {
  const getTabbableElements = () => {
    const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
    const pageHighlightElements = pdfHighlighters[tabIndex].querySelectorAll(
      ".highlight-layer-wrapper"
    );
    const tabbableElements = Array.from(pageHighlightElements).reduce(
      (acc, current) => {
        const elements = current.querySelectorAll(
          'button:not([disabled]), [tabindex="0"]'
        );
        acc = [...acc, elements[0]];
        return acc;
      },
      [] as Element[]
    );
    return tabbableElements;
  };

  const getRemoveRedactionBtn = () => {
    const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
    const removeBtn = pdfHighlighters[tabIndex].querySelector("#remove-btn");
    return removeBtn;
  };

  const getRemoveAllRedactionBtn = () => {
    const pdfHighlighters = document.querySelectorAll(".govuk-tabs__panel");
    const removeAllRedactionBtn = pdfHighlighters[tabIndex].querySelector(
      "#btn-link-removeAll"
    );
    return removeAllRedactionBtn;
  };

  const getReportIssueBtn = () => {
    const pdfHighlighters = document.querySelectorAll(".govuk-tabs__panel");
    const reportIssueBtn =
      pdfHighlighters[tabIndex].querySelector("#btn-report-issue");
    return reportIssueBtn;
  };
  const getOpenPdfBtn = () => {
    const pdfHighlighters = document.querySelectorAll(".govuk-tabs__panel");
    const openPdfBtn = pdfHighlighters[tabIndex].querySelector("#btn-open-pdf");
    return openPdfBtn;
  };

  const activeButtonIndex = useRef(0);

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      if (tabId === activeTabId) {
        const tabbableElements = getTabbableElements();
        const removeBtn = getRemoveRedactionBtn();
        const removeAllRedactionBtn = getRemoveAllRedactionBtn();
        const reportIssueBtn = getReportIssueBtn();
        const openPdfBtn = getOpenPdfBtn();
        const entryButton = (reportIssueBtn as HTMLButtonElement).disabled
          ? openPdfBtn
          : reportIssueBtn;

        if (document.activeElement) {
          if (
            (e.code === "Tab" || e.key === "Tab") &&
            !e.shiftKey &&
            tabbableElements
          ) {
            if (document.activeElement === entryButton) {
              (tabbableElements[0] as HTMLElement).focus();
              (tabbableElements[0] as HTMLElement).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              activeButtonIndex.current = 0;
              if (removeBtn) {
                (removeBtn as HTMLElement).style.visibility = "visible";
              }
              e.preventDefault();
              return;
            }
            if (
              document.activeElement === removeBtn &&
              activeButtonIndex.current < tabbableElements.length - 1
            ) {
              activeButtonIndex.current = activeButtonIndex.current + 1;
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).focus();
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              e.preventDefault();
              return;
            }
            if (
              Array.from(tabbableElements).includes(document.activeElement) &&
              removeBtn
            ) {
              activeButtonIndex.current = Array.from(tabbableElements).indexOf(
                document.activeElement
              );
              (removeBtn as HTMLElement).focus();
              (removeBtn as HTMLElement).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              e.preventDefault();
              return;
            }
            if (
              document.activeElement === removeBtn &&
              activeButtonIndex.current === tabbableElements.length - 1
            ) {
              (removeAllRedactionBtn as HTMLElement).focus();
              (removeAllRedactionBtn as HTMLElement).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              (removeBtn as HTMLElement).style.visibility = "hidden";
              e.preventDefault();
              return;
            }
          }
          if (
            (e.code === "Tab" || e.key === "Tab") &&
            tabbableElements &&
            e.shiftKey
          ) {
            if (document.activeElement === removeAllRedactionBtn) {
              (
                tabbableElements[tabbableElements.length - 1] as HTMLElement
              ).focus();
              (
                tabbableElements[tabbableElements.length - 1] as HTMLElement
              ).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              activeButtonIndex.current = tabbableElements.length - 1;
              if (removeBtn) {
                (removeBtn as HTMLElement).style.visibility = "visible";
              }
              e.preventDefault();
              return;
            }
            if (document.activeElement === removeBtn) {
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).focus();
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              e.preventDefault();
              return;
            }
            if (
              Array.from(tabbableElements).includes(document.activeElement) &&
              activeButtonIndex.current > 0
            ) {
              activeButtonIndex.current = activeButtonIndex.current - 1;
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).focus();
              (
                tabbableElements[activeButtonIndex.current] as HTMLElement
              ).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              e.preventDefault();
              return;
            }
            if (document.activeElement === tabbableElements[0]) {
              (entryButton as HTMLElement).focus();
              (entryButton as HTMLElement).scrollIntoView({
                behavior: "smooth",
                block: "center",
              });
              if (removeBtn) {
                (removeBtn as HTMLElement).style.visibility = "hidden";
              }
              e.preventDefault();
              return;
            }
          }
        }
      }
    },
    [activeTabId]
  );

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
