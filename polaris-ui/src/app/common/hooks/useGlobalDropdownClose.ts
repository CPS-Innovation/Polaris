import { useCallback, useEffect } from "react";
import { useFocusTrap } from "./useFocusTrap";

export const useGlobalDropdownClose = (
  dropDownBtnRef: React.MutableRefObject<HTMLElement | null>,
  panelRef: React.MutableRefObject<HTMLElement | null>,
  setButtonOpen: React.Dispatch<React.SetStateAction<boolean>>,
  panelId: string
) => {
  useFocusTrap(panelId);

  const handleOutsideClick = useCallback(
    (event: MouseEvent) => {
      if (
        event.target === dropDownBtnRef.current ||
        dropDownBtnRef.current?.contains(event.target as Node)
      ) {
        return;
      }

      const isTargetInsidePanel =
        panelRef.current &&
        event.target &&
        panelRef.current.contains(event.target as Node);

      const isTargetFlaggedAsExempt =
        event.target instanceof HTMLElement &&
        event.target.hasAttribute("data-prevent-global-close");

      if (!(isTargetInsidePanel || isTargetFlaggedAsExempt)) {
        setButtonOpen(false);
        event.stopPropagation();
      }
    },
    [dropDownBtnRef, panelRef, setButtonOpen]
  );

  const keyDownHandler = useCallback(
    (event: KeyboardEvent) => {
      if (event.code === "Escape" && panelRef.current) {
        setButtonOpen(false);
        dropDownBtnRef.current?.focus();
      }
    },
    [dropDownBtnRef, panelRef, setButtonOpen]
  );

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    document.addEventListener("click", handleOutsideClick);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
      document.removeEventListener("click", handleOutsideClick);
    };
  }, [keyDownHandler, handleOutsideClick]);
};
