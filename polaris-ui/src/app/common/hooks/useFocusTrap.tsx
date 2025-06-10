import { useEffect, useCallback } from "react";

export const useFocusTrap = (id = "#modal") => {
  const getTabbableElements = useCallback(() => {
    if (!document.querySelector(id)) {
      return;
    }
    const tabbableElements = document
      .querySelector(id)
      ?.querySelectorAll(
        'a[href], area[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), [tabindex="0"], summary:not([disabled])'
      );

    return tabbableElements;
  }, [id]);

  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      const tabbableElements = getTabbableElements();

      if ((e.code === "Tab" || e.key === "Tab") && tabbableElements) {
        if (e.shiftKey) {
          if (document.activeElement === tabbableElements[0]) {
            (
              tabbableElements[tabbableElements.length - 1] as HTMLElement
            ).focus();
            e.preventDefault();
          }
        }
        if (
          !e.shiftKey &&
          document.activeElement ===
            tabbableElements[tabbableElements.length - 1]
        ) {
          (tabbableElements[0] as HTMLElement).focus();
          e.preventDefault();
        }
      }
    },
    [getTabbableElements]
  );

  useEffect(() => {
    const setFirstElementFocus = () => {
      const tabbableElements = getTabbableElements();
      if (tabbableElements?.length) {
        setTimeout(() => {
          (tabbableElements[0] as HTMLElement).focus();
        }, 10);
      }
    };
    setFirstElementFocus();
  }, [getTabbableElements]);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
