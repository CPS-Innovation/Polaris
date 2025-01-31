import { EventEmitter } from "../stencil-public-runtime";
export declare class CpsGlobalNavExperimental {
    /**
     * The text to appear at the start of the second row
     */
    name: string;
    forceEnvironment: string;
    cpsGlobalNavEvent: EventEmitter<string>;
    leadDefendantPresentationName: string;
    leadDefendantClassName: string;
    componentWillLoad(): Promise<void>;
    componentDidLoad(): Promise<void>;
    componentWillRender(): void;
    emitWindowEvent(): void;
    render(): any;
}
