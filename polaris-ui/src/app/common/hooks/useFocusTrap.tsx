import { useEffect, useCallback, useState } from "react";

export const useFocusTrap = (id: string = "#modal") => {
  const [start, setStart] = useState(false);

  const getTabbableElements = () => {
    const tabbableElements = document
      .querySelector(id)
      ?.querySelectorAll(
        'a[href], area[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled]), [tabindex="0"], summary:not([disabled])'
      );

    return tabbableElements;
  };
  const keyDownHandler = useCallback(
    (e: KeyboardEvent) => {
      const tabbableElements = getTabbableElements();
      if (!start) {
        setStart(true);
      }
      if (tabbableElements && tabbableElements.length === 1) {
        (tabbableElements[0] as HTMLElement).focus();
      }

      if ((e.code === "Tab" || e.key === "Tab") && tabbableElements) {
        if (e.shiftKey) {
          if (document.activeElement === tabbableElements[0]) {
            (
              tabbableElements[tabbableElements.length - 1] as HTMLElement
            ).focus();
            e.preventDefault();
          }
        } else {
          if (
            document.activeElement ===
            tabbableElements[tabbableElements.length - 1]
          ) {
            (tabbableElements[0] as HTMLElement).focus();
            e.preventDefault();
          }
        }
      }
    },
    [start]
  );

  useEffect(() => {
    const tabbableElements = getTabbableElements();
    if (!start && tabbableElements) {
      (tabbableElements[0] as HTMLElement).focus();
    }
  }, [start]);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
