import { useEffect, useCallback, useRef } from "react";
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
    if (activeTextLayerChildIndex.current === -1) {
      return getNonEmptyTextContentElements(textLayers[0].children);
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
  }, []);

  const getTextToSelect = useCallback(
    (keyCode: string) => {
      const textLayerChildren = getTextLayerChildren();
      if (
        keyCode === "KeyH" &&
        activeTextLayerChildIndex.current !== -1 &&
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
    [getTextLayerChildren]
  );

  const getFirstNonEmptySpanIndex = (child: Element) => {
    if (!child.children.length) {
      return child;
    }
    let index = 0;
    while (
      !child.children[index].textContent?.trim() &&
      index < child.children.length - 1
    ) {
      index = index + 1;
    }
    return child.children[index];
  };

  const getNonEmptyTextContentElements = (elements: HTMLCollection) => {
    return Array.from(elements).filter((element) =>
      element.textContent?.trim()
    );
  };

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
        const child = getTextToSelect(e.code ?? e.key);
        (getFirstNonEmptySpanIndex(child) as HTMLElement).scrollIntoView({
          behavior: "smooth",
          block: "center",
        });
        const range = document.createRange();
        range.selectNodeContents(child);
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
