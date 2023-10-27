import { useEffect, useCallback, useMemo, useState, useRef } from "react";
import {
  getWordStartingIndices,
  getNonEmptyTextContentElements,
} from "./useDocumentFocusHelpers";
/**
 * This hook will take care of custom navigation and selection of starting letter of each word in each page
 * and making the focus trapped on the redact button if it is available. User can use key "," and "Shift" + key ","
 * to navigate forward and backward through the document words.
 */
const WORD_FOCUS_KEY = ",";
const ESCAPE_KEY_CODE = 27;
export const useDocumentFocus = (activeTabId: string | undefined) => {
  const getTextLayerChildren = () => {
    const textLayers = document
      .querySelector("#active-tab-panel")
      ?.querySelectorAll(".textLayer");
    if (!textLayers) {
      return [];
    }
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
  };
  const [keyPress, setKeyPress] = useState<{
    count: number;
    direction: "forward" | "backward" | "";
  }>({ count: 0, direction: "" });
  const [toggleRefresh, setToggleRefresh] = useState(false);
  const textLayerIndex = useRef(0);
  const activeTextLayerChildIndex = useRef(-1);
  const wordFirstLetterIndex = useRef(0);

  const sortedSpanElements = useMemo(() => {
    return getTextLayerChildren();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [toggleRefresh]);

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

  const getDocumentPanel = useCallback(() => {
    return document.querySelector(`#active-tab-panel`);
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

  const triggerEscapeKeyPress = useCallback(() => {
    document.dispatchEvent(
      new KeyboardEvent("keydown", {
        key: "Escape",
        keyCode: ESCAPE_KEY_CODE,
        which: ESCAPE_KEY_CODE,
        code: "Escape",
      })
    );
  }, []);

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      //key combination of ctrlKey and comma
      if (!e.ctrlKey || !(e.key === WORD_FOCUS_KEY)) {
        return;
      }

      e.preventDefault();
      e.shiftKey
        ? setKeyPress({ count: keyPress.count + 1, direction: "backward" })
        : setKeyPress({ count: keyPress.count + 1, direction: "forward" });

      //The textLayerIndex is used to keep track of the pages user has scrolled down which is used to get all the span children till that page progressively
      if (sortedSpanElements.length - 1 === activeTextLayerChildIndex.current) {
        textLayerIndex.current = textLayerIndex.current + 1;
        setToggleRefresh((toggleRefresh) => !toggleRefresh);
      }
    },
    [keyPress, sortedSpanElements.length]
  );
  // reset on tab change.
  useEffect(() => {
    setToggleRefresh((toggleRefresh) => !toggleRefresh);
    textLayerIndex.current = 0;
    setKeyPress({ count: -1, direction: "" });
    activeTextLayerChildIndex.current = -1;
    wordFirstLetterIndex.current = 0;
  }, [activeTabId]);

  useEffect(() => {
    const documentPanel = getDocumentPanel();
    if (!documentPanel || !keyPress.direction) {
      return;
    }
    if (!sortedSpanElements.length) {
      setToggleRefresh((toggleRefresh) => !toggleRefresh);
      return;
    }
    // this is to identify whether a re-calculation of the span elements is needed
    if (!document.contains(sortedSpanElements[0])) {
      setToggleRefresh((toggleRefresh) => !toggleRefresh);
      return;
    }
    (documentPanel as HTMLElement).focus();
    const textToSelect = getTextToSelect(
      sortedSpanElements,
      keyPress.direction
    );
    if (!textToSelect) {
      setToggleRefresh((toggleRefresh) => !toggleRefresh);
      return;
    }
    (textToSelect as HTMLElement).scrollIntoView({
      behavior: "smooth",
      block: "center",
    });
    triggerEscapeKeyPress();
    const range = document.createRange();
    range.selectNodeContents(textToSelect);
    range.setStart(textToSelect.firstChild, wordFirstLetterIndex.current);
    range.setEnd(textToSelect.firstChild, wordFirstLetterIndex.current + 1);
    document.getSelection()?.removeAllRanges();
    document.getSelection()?.addRange(range);
  }, [
    keyPress,
    getTextToSelect,
    getDocumentPanel,
    sortedSpanElements,
    sortedSpanElements.length,
    triggerEscapeKeyPress,
  ]);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
