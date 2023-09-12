import { useEffect, useCallback } from "react";

const SWITCH_CONTENT_AREA_KEY = ";";
export const useSwitchContentArea = () => {
  const getContentAreas = () => {
    const contentAreas = [
      document.querySelector("#side-panel"),
      document.querySelector("#document-tabs"),
      document.querySelector("#active-tab-panel"),
    ];

    return contentAreas.filter((contentArea) => contentArea);
  };

  const keyDownHandler = useCallback((e: KeyboardEvent) => {
    if (e.key !== SWITCH_CONTENT_AREA_KEY) return;
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
