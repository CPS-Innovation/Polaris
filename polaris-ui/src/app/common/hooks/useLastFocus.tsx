import { useEffect, useRef } from "react";
export const useLastFocus = (defaultFocusId?: string) => {
  const lastFocusElementRef = useRef<Element | null>(null);
  useEffect(() => {
    if (!lastFocusElementRef.current) {
      lastFocusElementRef.current = document.activeElement;
    }
    return () => {
      //only add last focus if the active element is set back to body element
      if (
        document.activeElement &&
        document.activeElement?.tagName !== "BODY"
      ) {
        return;
      }
      if (
        lastFocusElementRef.current &&
        document.contains(lastFocusElementRef.current)
      )
        (lastFocusElementRef.current as HTMLElement).focus();
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
