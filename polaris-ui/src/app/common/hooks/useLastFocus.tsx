import { useEffect, useRef } from "react";
export const useLastFocus = (defaultFocus?: HTMLElement) => {
  const lastFocusElementRef = useRef<Element>(document.activeElement);
  useEffect(() => {
    const ref = lastFocusElementRef.current;
    return () => {
      // if (ref && document.contains(ref)) {
      //   (ref as HTMLElement).focus();
      // } else {
      //   if (defaultFocus) {
      //     defaultFocus.focus();
      //   }
      // }
    };
  }, [defaultFocus]);
};
