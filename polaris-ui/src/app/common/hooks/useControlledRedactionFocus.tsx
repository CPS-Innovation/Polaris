import { useEffect, useCallback, useRef } from "react";

export const useControlledRedactionFocus = () => {
  const getTabbableElements = () => {
    const pageHighlightElements = document.querySelectorAll(
      ".PdfHighlighter__highlight-layer"
    );

    const tabbableElements = Array.from(pageHighlightElements).reduce(
      (acc, current) => {
        const elements = current.querySelectorAll(
          'button:not([disabled]), [tabindex="0"]'
        );
        acc = [...acc, ...elements];
        return acc;
      },
      [] as Element[]
    );

    return tabbableElements;
  };
  const activeButtonIndex = useRef(0);

  const keyDownHandler = useCallback((e: KeyboardEvent) => {
    const tabbableElements = getTabbableElements();

    const removeBtn = document.getElementById("remove-btn");
    if (document.activeElement) {
      if (
        (e.code === "Tab" || e.key === "Tab") &&
        !e.shiftKey &&
        tabbableElements
      ) {
        console.log("keyDownHandler>>", document.activeElement);
        if (
          document.activeElement === removeBtn &&
          activeButtonIndex.current < tabbableElements.length - 1
        ) {
          (
            tabbableElements[activeButtonIndex.current + 1] as HTMLElement
          ).focus();
          (
            tabbableElements[activeButtonIndex.current + 1] as HTMLElement
          ).scrollIntoView({
            behavior: "smooth",
            block: "center",
          });
          e.preventDefault();
          return;
        }
        if (Array.from(tabbableElements).includes(document.activeElement)) {
          activeButtonIndex.current = Array.from(tabbableElements).indexOf(
            document.activeElement
          );
          removeBtn?.focus();
          (removeBtn as HTMLElement).scrollIntoView({
            behavior: "smooth",
            block: "center",
          });
          e.preventDefault();
        }
      }
      if (
        (e.code === "Tab" || e.key === "Tab") &&
        tabbableElements &&
        e.shiftKey
      ) {
        if (document.activeElement === removeBtn) {
          (tabbableElements[activeButtonIndex.current] as HTMLElement).focus();
          (
            tabbableElements[activeButtonIndex.current] as HTMLElement
          ).scrollIntoView({
            behavior: "smooth",
            block: "center",
          });
          e.preventDefault();
          return;
        }
      }
    }
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
