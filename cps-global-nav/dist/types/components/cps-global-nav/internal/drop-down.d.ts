import { LinkProps } from "./LinkProps";
export declare class DropDown {
    label: string;
    links: LinkProps[];
    menuAlignment: "left" | "right";
    opened: boolean;
    checkForClickOutside(ev: MouseEvent): void;
    private topLevelHyperlink;
    private handleLabelClick;
    render(): any;
}
