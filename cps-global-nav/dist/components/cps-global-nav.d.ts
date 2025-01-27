import type { Components, JSX } from "../types/components";

interface CpsGlobalNav extends Components.CpsGlobalNav, HTMLElement {}
export const CpsGlobalNav: {
    prototype: CpsGlobalNav;
    new (): CpsGlobalNav;
};
/**
 * Used to define this component and all nested components recursively.
 */
export const defineCustomElement: () => void;
