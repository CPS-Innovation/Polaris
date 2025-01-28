import { LinkCode, MatchedPathMatcher } from "../../context/LocationConfig";
type LinkHelperArg = {
    code: LinkCode;
    label: string;
    children?: LinkCode[];
    openInNewTab?: boolean;
};
export declare class CpsGlobalNav {
    /**
     * The text to appear at the start of the second row
     */
    name: string;
    config: MatchedPathMatcher;
    componentWillLoad(): Promise<void>;
    linkHelper: ({ code, label, children, openInNewTab }: LinkHelperArg) => {
        label: string;
        href: string;
        selected: boolean;
        openInNewTab: boolean;
    };
    render(): any;
}
export {};
