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
      if (lastFocusElement && document.contains(lastFocusElement)) {
        const elm = lastFocusElement as HTMLElement;
        if (elm.classList.contains("textLayer")) return;
        elm.focus();
      } else {
        if (defaultFocusId) {
          const defaultElement = document.querySelector(
            `${defaultFocusId}`
          ) as HTMLElement;
          console.log({ defaultElement });
          if (defaultElement) defaultElement.focus();
        }
      }
    };
  }, []);
};
