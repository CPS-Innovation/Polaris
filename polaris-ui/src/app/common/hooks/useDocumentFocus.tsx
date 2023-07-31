import { useEffect, useCallback, useRef } from "react";
import {
  getWordStartingIndices,
  getFirstNonEmptySpanIndex,
  getNonEmptyTextContentElements,
} from "./useDocumentFocusHelpers";
/**
 * This hook will take care of custom navigation and selection of all the span elements in each page
 * and making the focus trapped on the redact button if it is available. User can use "keyH" and "keyG"
 * to navigate through the document span elements.
 */
export const useDocumentFocus = (
  tabId: string,
  activeTabId: string | undefined,
  tabIndex: number
) => {
  const activeTextLayerChildIndex = useRef(-1);
  const textLayerIndex = useRef(0);
  const wordFirstLetterIndex = useRef(0);

  /*
  Each textLayer child might have more than one word as text content and need to navigate our focus to start of the each word, 
  this function will identify this and update the wordFirstLetterIndex in than text content
  */
  const hasNextWordIndex = (textLayerChildren: any[], keyCode: string) => {
    const sentence =
      textLayerChildren[activeTextLayerChildIndex.current]?.textContent;
    const startIndexes = getWordStartingIndices(sentence);
    const currentIndex = startIndexes.findIndex(
      (value) => value === wordFirstLetterIndex.current
    );
    if (keyCode === "KeyH") {
      if (currentIndex < startIndexes.length - 1) {
        wordFirstLetterIndex.current = startIndexes[currentIndex + 1];
        return true;
      }
      wordFirstLetterIndex.current = 0;
    }
    if (keyCode === "KeyG") {
      if (currentIndex > 0) {
        wordFirstLetterIndex.current = startIndexes[currentIndex - 1];
        return true;
      }
      const oldSentence =
        textLayerChildren[activeTextLayerChildIndex.current - 1]?.textContent;
      const previousIndexes = getWordStartingIndices(oldSentence);
      wordFirstLetterIndex.current = previousIndexes.length
        ? previousIndexes[previousIndexes.length - 1]
        : 0;
    }
    return false;
  };

  const getRedactBtn = useCallback(() => {
    const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
    const redactBtn = pdfHighlighters[tabIndex].querySelector("#btn-redact");
    return redactBtn;
  }, [tabIndex]);

  const getDocumentPanel = useCallback(() => {
    return document.querySelector(`#panel-${tabIndex}`);
  }, [tabIndex]);

  const getTextLayerChildren = useCallback(() => {
    const textLayers = document.querySelectorAll(".textLayer");
    const children = Array.from(textLayers).reduce(
      (acc: any[], textLayer: Element, index) => {
        if (index > textLayerIndex.current) {
          return acc;
        }
        acc = [...acc, ...getNonEmptyTextContentElements(textLayer.children)];
        return acc;
      },
      []
    );

    return children;
  }, []);

  const getTextToSelect = useCallback(
    (textLayerChildren: any[], keyCode: string) => {
      const hasNextWord = hasNextWordIndex(textLayerChildren, keyCode);
      // if there are more words on the same child continue with the same child
      if (hasNextWord) {
        return textLayerChildren[activeTextLayerChildIndex.current];
      }
      if (
        keyCode === "KeyH" &&
        activeTextLayerChildIndex.current >= textLayerChildren.length - 1
      ) {
        activeTextLayerChildIndex.current = textLayerChildren.length - 1;
      } else if (
        keyCode === "KeyH" &&
        activeTextLayerChildIndex.current < textLayerChildren.length
      ) {
        activeTextLayerChildIndex.current =
          activeTextLayerChildIndex.current + 1;
      }

      if (keyCode === "KeyG" && activeTextLayerChildIndex.current <= 0) {
        activeTextLayerChildIndex.current = 0;
      } else if (keyCode === "KeyG" && activeTextLayerChildIndex.current > 0) {
        activeTextLayerChildIndex.current =
          activeTextLayerChildIndex.current - 1;
      }
      const selection = textLayerChildren[activeTextLayerChildIndex.current];
      return selection;
    },
    []
  );

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      if (activeTabId === tabId) {
        if (e.code === "Tab" || e.key === "Tab") {
          const redactBtn = getRedactBtn();
          if (redactBtn) {
            (redactBtn as HTMLElement).focus();
            e.preventDefault();
          }
        }
        if (!(e.code === "KeyG" || e.code === "KeyH")) {
          return;
        }
        const documentPanel = getDocumentPanel();
        if (!documentPanel) {
          return;
        }
        (documentPanel as HTMLElement).focus();

        const textLayerChildren = getTextLayerChildren();
        //The textLayerIndex is used to keep track of the pages user has scrolled down which is used to get all the span children till that page progressively
        if (
          textLayerChildren.length - 1 ===
          activeTextLayerChildIndex.current
        ) {
          textLayerIndex.current = textLayerIndex.current + 1;
        }
        const child = getTextToSelect(textLayerChildren, e.code ?? e.key);
        (getFirstNonEmptySpanIndex(child) as HTMLElement).scrollIntoView({
          behavior: "smooth",
          block: "center",
        });
        const range = document.createRange();
        range.selectNodeContents(child);
        range.setStart(child.firstChild, wordFirstLetterIndex.current);
        range.setEnd(child.firstChild, wordFirstLetterIndex.current + 1);
        document.getSelection()?.removeAllRanges();
        document.getSelection()?.addRange(range);
      }
    },
    [
      activeTabId,
      tabId,
      getRedactBtn,
      getDocumentPanel,
      getTextToSelect,
      getTextLayerChildren,
    ]
  );

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
