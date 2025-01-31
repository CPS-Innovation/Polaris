import { EventEmitter } from "../../../stencil-public-runtime";
export declare class NavLink {
    label: string;
    href: string;
    selected: boolean;
    disabled: boolean;
    openInNewTab?: boolean;
    cpsGlobalNavEvent: EventEmitter<string>;
    emitEvent: () => void;
    launchNewTab: () => void;
    render(): any;
}
