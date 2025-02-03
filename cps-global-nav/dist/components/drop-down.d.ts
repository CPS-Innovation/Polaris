import type { Components, JSX } from "../types/components";

interface DropDown extends Components.DropDown, HTMLElement {}
export const DropDown: {
    prototype: DropDown;
    new (): DropDown;
};
/**
 * Used to define this component and all nested components recursively.
 */
export const defineCustomElement: () => void;
