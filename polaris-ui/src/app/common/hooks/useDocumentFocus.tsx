import { useEffect, useCallback, useRef } from "react";
import {
  getWordStartingIndices,
  getNonEmptyTextContentElements,
} from "./useDocumentFocusHelpers";
/**
 * This hook will take care of custom navigation and selection of starting letter of each word in each page
 * and making the focus trapped on the redact button if it is available. User can use "key W" and " Shift + key W"
 * to navigate forward and backward through the document words.
 */
const WORD_FOCUS_KEY = ",";
const TAB_KEY_CODE = "Tab";
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
  const hasNextWordIndex = (
    textLayerChildren: any[],
    direction: "forward" | "backward"
  ) => {
    const sentence =
      textLayerChildren[activeTextLayerChildIndex.current]?.textContent;
    const startIndexes = getWordStartingIndices(sentence);
    const currentIndex = startIndexes.findIndex(
      (value) => value === wordFirstLetterIndex.current
    );
    if (direction === "forward") {
      if (currentIndex < startIndexes.length - 1) {
        wordFirstLetterIndex.current = startIndexes[currentIndex + 1];
        return true;
      }
      wordFirstLetterIndex.current = 0;
    }
    if (direction === "backward") {
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
    return document.querySelector(`#active-tab-panel`);
  }, []);

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
    (textLayerChildren: any[], direction: "forward" | "backward") => {
      const hasNextWord = hasNextWordIndex(textLayerChildren, direction);
      // if there are more words on the same child continue with the same child
      if (hasNextWord) {
        return textLayerChildren[activeTextLayerChildIndex.current];
      }
      if (direction === "forward") {
        if (activeTextLayerChildIndex.current >= textLayerChildren.length - 1) {
          activeTextLayerChildIndex.current = textLayerChildren.length - 1;
        } else if (
          activeTextLayerChildIndex.current < textLayerChildren.length
        ) {
          activeTextLayerChildIndex.current =
            activeTextLayerChildIndex.current + 1;
        }
      }

      if (direction === "backward") {
        if (activeTextLayerChildIndex.current <= 0) {
          activeTextLayerChildIndex.current = 0;
        } else if (activeTextLayerChildIndex.current > 0) {
          activeTextLayerChildIndex.current =
            activeTextLayerChildIndex.current - 1;
        }
      }
      const selection = textLayerChildren[activeTextLayerChildIndex.current];
      return selection;
    },
    []
  );

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      //this is temporary hack supply the keycode from console, for any further live test of key combination.
      const WORD_FOCUS_CODE = (window as any).wordFocusCode ?? "Comma";
      if (activeTabId !== tabId) {
        return;
      }

      if (e.code === TAB_KEY_CODE || e.key === TAB_KEY_CODE) {
        const redactBtn = getRedactBtn();
        if (redactBtn) {
          (redactBtn as HTMLElement).focus();
          e.preventDefault();
        }
      }
      if (!(e.code === WORD_FOCUS_CODE || e.key === WORD_FOCUS_KEY)) {
        return;
      }
      console.log("helloo111");
      e.preventDefault();
      const documentPanel = getDocumentPanel();
      if (!documentPanel) {
        return;
      }
      (documentPanel as HTMLElement).focus();
      const textLayerChildren = getTextLayerChildren();
      //The textLayerIndex is used to keep track of the pages user has scrolled down which is used to get all the span children till that page progressively
      if (textLayerChildren.length - 1 === activeTextLayerChildIndex.current) {
        textLayerIndex.current = textLayerIndex.current + 1;
      }
      const direction = e.shiftKey ? "backward" : "forward";
      const textToSelect = getTextToSelect(textLayerChildren, direction);
      (textToSelect as HTMLElement).scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
      const range = document.createRange();
      range.selectNodeContents(textToSelect);
      range.setStart(textToSelect.firstChild, wordFirstLetterIndex.current);
      range.setEnd(textToSelect.firstChild, wordFirstLetterIndex.current + 1);
      document.getSelection()?.removeAllRanges();
      document.getSelection()?.addRange(range);
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
