import { useEffect, useRef } from "react";
export const useLastFocus = (defaultFocus?: HTMLElement) => {
  const lastFocusElementRef = useRef<Element>(document.activeElement);
  useEffect(() => {
    const ref = lastFocusElementRef.current;
    return () => {
      //only add last focus if there is no active focus element other than document.body
      if (
        document.activeElement &&
        document.activeElement?.tagName !== "BODY"
      ) {
        return;
      }
      if (ref && document.contains(ref)) {
        setTimeout(() => {
          (ref as HTMLElement).focus();
        }, 0);
      } else {
        if (defaultFocus) {
          setTimeout(() => {
            defaultFocus.focus();
          }, 0);
        }
      }
    };
  }, [defaultFocus]);
};
