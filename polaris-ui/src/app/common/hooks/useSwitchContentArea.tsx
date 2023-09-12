import { useEffect, useCallback } from "react";

const SWITCH_CONTENT_AREA_KEY = ";";
/**
 * This custom hook is used to switch the focus between three main content areas in the case-details page
 */
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

    e.preventDefault();
    const contentAreas = getContentAreas();
    const activeAreaIndex = contentAreas.findIndex(
      (contentArea) => document.activeElement === contentArea
    );
    if (activeAreaIndex < contentAreas.length - 1) {
      (contentAreas[activeAreaIndex + 1] as HTMLElement).focus();
      return;
    }
    (contentAreas[0] as HTMLElement).focus();
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
    };
  }, [keyDownHandler]);
};
