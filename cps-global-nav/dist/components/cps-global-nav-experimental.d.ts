import type { Components, JSX } from "../types/components";

interface CpsGlobalNavExperimental extends Components.CpsGlobalNavExperimental, HTMLElement {}
export const CpsGlobalNavExperimental: {
    prototype: CpsGlobalNavExperimental;
    new (): CpsGlobalNavExperimental;
};
/**
 * Used to define this component and all nested components recursively.
 */
export const defineCustomElement: () => void;
