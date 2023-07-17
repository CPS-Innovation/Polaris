import { useEffect, useCallback, useRef } from "react";
/**
 * This hook will take care of custom focus control on unsaved redaction buttons and remove redaction button in a document,
 * by ignoring all other tab-able elements within the document when user navigates through the unsaved redaction,
 * using keyboard Tab or (shift+tab) key.
 */
export const useDocumentFocus = (
  tabId: string,
  activeTabId: string | undefined,
  tabIndex: number
) => {
  const getChildAtPosition = (x: number, y: number, textLayer: Element) => {
    console.log("getChildAtPosition:x>>", x);
    console.log("getChildAtPosition:y>>", y);
    const child = Array.from(textLayer.children).find((element: any) => {
      console.log("element>>", element.textContent);
      if (!element.textContent.trim()) {
        return false;
      }
      const bRect = element.getBoundingClientRect();
      const elementX = bRect.x + bRect.width / 2;
      const elementY = bRect.y + 12;
      console.log(`cursorX>>:${x}, cursorY>>:${y}`);
      console.log(`elementX>>:${elementX}, elementY>>:${elementY}`);
      if (Math.abs(x - elementX) < 20 && Math.abs(y - elementY) < 20)
        return true;
    });

    console.log("child>>", child);
    if (child) {
      console.log(
        "child>>getBoundingClientRect()",
        child.getBoundingClientRect()
      );

      // .trigger("mousedown")
      // .then(() => {
      //   const el = element[0]
      //   const document = el.ownerDocument
      //   const range = document.createRange()
      //   range.selectNodeContents(el)
      //   document.getSelection()?.removeAllRanges()
      //   document.getSelection()?.addRange(range)
      // })
      // .trigger("mouseup")
      child.dispatchEvent(
        new MouseEvent("mousedown", {
          bubbles: true,
        })
      );
      const range = document.createRange();
      range.selectNodeContents(child);
      document.getSelection()?.removeAllRanges();
      document.getSelection()?.addRange(range);
      child.dispatchEvent(
        new MouseEvent("mouseup", {
          bubbles: true,
        })
      );
    }
  };

  // This function will be called when the mouse is clicked on the absolutely positioned element.
  const myFunction = (event: MouseEvent) => {
    console.log("firing on parent element...0000X", event.clientX);
    console.log("firing on parent element...0000Y", event.clientY);
    // Get the position of the absolutely positioned element.
    // Get the position of the absolutely positioned element.
    var elementPosition = (event.target as HTMLElement).getBoundingClientRect();

    const textLayer = document.querySelectorAll(".textLayer")[0];

    // getChildAtPosition(elementPosition.x, elementPosition.y, textLayer);

    // // Find the parent element of the absolutely positioned element that you want to fire the mouse event on.
    // var parentElement = document.querySelectorAll(".PdfHighlighter")[tabIndex];

    // // Calculate the position of the mouse event in the parent element.
    // var mousePositionInParent = {
    //   x: elementPosition.x + event.clientX,
    //   y: elementPosition.y + event.clientY,
    // };

    // // Fire a mouse event on the parent element at the calculated position.
    // if (parentElement) {
    //   console.log("firing on parent element...111", mousePositionInParent);
    //   parentElement.dispatchEvent(
    //     new MouseEvent("dbclick", {
    //       bubbles: true,
    //       clientX: 100,
    //       clientY: 100,
    //     })
    //   );
    // }
  };

  const addCustomCursor = () => {
    if (activeTabId === tabId) {
      console.log("adding custom cursor....00000");
      const pdfHighlighters = document.querySelectorAll(".PdfHighlighter");
      if (tabIndex >= pdfHighlighters.length) {
        return;
      }
      console.log("adding custom cursor....", tabIndex);
      var cursorDiv = document.createElement("button");
      cursorDiv.onclick = () => {
        console.log("hiiiiii");
      };
      cursorDiv.id = "fake-cursor";
      cursorDiv.textContent = "cursor";
      cursorDiv.style.background = "black";
      cursorDiv.style.width = "50px";
      cursorDiv.style.height = "10px";
      cursorDiv.style.top = "10px";

      cursorDiv.style.left = "90px";
      cursorDiv.style.position = "absolute";
      pdfHighlighters[tabIndex].appendChild(cursorDiv);
      cursorDiv.addEventListener("click", myFunction);
    }
  };

  const activeButtonIndex = useRef(0);
  const textLayerIndex = useRef(0);

  const triggerMouseDblClick = () => {
    const element = document.getElementById("fake-cursor");
    // Create a MouseEvent object
    let event = new MouseEvent("click", {
      bubbles: true,
      cancelable: true,
    });

    // Set the event properties
    // event.initEvent("dblclick", true, true);
    console.log("trigering mouse event");
    // Fire the event on the element
    if (element) element.dispatchEvent(event);
  };

  const getDocumentPanel = () => {
    return document.querySelector(`#panel-${tabIndex}`);
  };

  const getTextToSelect = (keyCode: string) => {
    const textLayerChildren = getTextLayerChildren();

    console.log("textLayerChildren>>", textLayerChildren);
    // do {
    if (
      keyCode === "KeyH" &&
      activeButtonIndex.current > textLayerChildren.length - 1
    ) {
      console.log("helloooo");
      activeButtonIndex.current = 0;
      //   continue;
    }
    if (keyCode === "KeyG" && activeButtonIndex.current <= 0) {
      activeButtonIndex.current = textLayerChildren.length - 1;
      //   continue;
    }
    if (
      keyCode === "KeyH" &&
      activeButtonIndex.current < textLayerChildren.length
    ) {
      activeButtonIndex.current = activeButtonIndex.current + 1;
    } else if (keyCode === "KeyG" && activeButtonIndex.current > 0) {
      activeButtonIndex.current = activeButtonIndex.current - 1;
    }
    // } while (
    //   textLayerChildren.length &&
    //   !textLayerChildren[activeButtonIndex.current]?.textContent?.trim()
    // );

    const selection = textLayerChildren[activeButtonIndex.current];

    return selection;
    // return null;
  };

  const getFirstNonEmptySpanIndex = (child: Element) => {
    // return child;
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

  const getTextLayerChildren = () => {
    const textLayers = document.querySelectorAll(".textLayer");
    if (activeButtonIndex.current === -1) {
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
  };

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      if (activeTabId === tabId) {
        if (!(e.code === "KeyG" || e.code === "KeyH")) {
          return;
        }
        // console.log("e.code", activeButtonIndex.current);
        // console.log("e.key", e.key);
        // console.log("e.code", e.code);
        const documentPanel = getDocumentPanel();
        if (!documentPanel) {
          return;
        }

        const textLayerChildren = getTextLayerChildren();
        console.log("textLayerChildren>>111", textLayerChildren.length);
        console.log("activeButtonIndex.current>111", activeButtonIndex.current);
        if (textLayerChildren.length - 1 === activeButtonIndex.current) {
          textLayerIndex.current = textLayerIndex.current + 1;
        }
        const child = getTextToSelect(e.code ?? e.key);
        if (!child) {
          return;
        }

        (documentPanel as HTMLElement).focus();

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
    [activeTabId]
  );

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);

  //   useEffect(() => {
  //     if (!document.getElementById("fake-cursor")) addCustomCursor();
  //   }, [activeTabId]);
};
