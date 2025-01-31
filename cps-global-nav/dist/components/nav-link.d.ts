import type { Components, JSX } from "../types/components";

interface NavLink extends Components.NavLink, HTMLElement {}
export const NavLink: {
    prototype: NavLink;
    new (): NavLink;
};
/**
 * Used to define this component and all nested components recursively.
 */
export const defineCustomElement: () => void;
