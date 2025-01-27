import { EventEmitter } from "../../stencil-public-runtime";
import { LinkCode, MatchedPathMatcher } from "../../context/LocationConfig";
export declare class CpsGlobalNav {
    /**
     * The text to appear at the start of the second row
     */
    name: string;
    forceEnvironment: string;
    cpsGlobalNavEvent: EventEmitter<string>;
    config: MatchedPathMatcher;
    componentWillLoad(): Promise<void>;
    linkHelper: (code: LinkCode, label: string) => {
        label: string;
        href: string;
        selected: boolean;
    };
    emitWindowEvent(): void;
    render(): any;
}
