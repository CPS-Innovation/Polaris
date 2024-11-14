import type { Page } from "../types";
export declare const getDocument: (element: Element) => Document;
export declare const getWindow: (element: Element) => typeof window;
export declare const isHTMLElement: (element: Element | null) => element is HTMLElement;
export declare const isHTMLCanvasElement: (element: Element) => element is HTMLCanvasElement;
export declare const getPageFromElement: (target: HTMLElement) => Page | null;
export declare const getPagesFromRange: (range: Range) => Page[];
export declare const findOrCreateContainerLayer: (container: HTMLElement, className: string) => Element;
