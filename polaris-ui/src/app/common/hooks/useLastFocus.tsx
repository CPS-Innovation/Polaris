import { useEffect, useRef } from "react";
export const useLastFocus = (defaultFocusId?: string) => {
  const lastFocusElementRef = useRef<Element>(document.activeElement);
  useEffect(() => {
    const lastFocusElement = lastFocusElementRef.current;
    return () => {
      //only add last focus if the active element is set back to body element
      if (
        document.activeElement &&
        document.activeElement?.tagName !== "BODY"
      ) {
        return;
      }
      if (lastFocusElement && document.contains(lastFocusElement))
        (lastFocusElement as HTMLElement).focus();
      else {
        if (defaultFocusId) {
          const defaultElement = document.querySelector(
            `${defaultFocusId}`
          ) as HTMLElement;
          if (defaultElement) defaultElement.focus();
        }
      }
    };
  }, []);
};
