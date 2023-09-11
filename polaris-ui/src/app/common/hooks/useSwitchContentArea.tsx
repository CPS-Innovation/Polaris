import { useEffect, useCallback } from "react";

const SWITCH_CONTENT_AREA_KEY = ";";
export const useSwitchContentArea = (
  tabId?: string,
  activeTabId?: string | undefined,
  tabIndex?: number
) => {
  const getContentAreas = () => {
    const contentAreas = [
      document.querySelector("#content-area-1"),
      document.querySelector("#content-area-2"),
      document.querySelector("#content-area-3"),
    ];

    return contentAreas.filter((contentArea) => contentArea);
  };

  const keyDownHandler = useCallback((e: KeyboardEvent) => {
    console.log(e.code);

    if (e.key === SWITCH_CONTENT_AREA_KEY) {
      const contentAreas = getContentAreas();
      const activeAreaIndex = contentAreas.findIndex(
        (contentArea) => document.activeElement === contentArea
      );
      if (activeAreaIndex < contentAreas.length - 1) {
        (contentAreas[activeAreaIndex + 1] as HTMLElement).focus();
        return;
      }
      (contentAreas[0] as HTMLElement).focus();
      e.preventDefault();
    }
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
