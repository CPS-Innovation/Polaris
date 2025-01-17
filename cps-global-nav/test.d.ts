export * from "./dist/types/index";

declare global {
  export namespace JSX {
    interface IntrinsicElements {
      ["cps-global-nav"]: any;
    }
  }
}
