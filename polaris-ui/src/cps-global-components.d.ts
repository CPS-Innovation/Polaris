declare global {
  namespace React {
    namespace JSX {
      interface IntrinsicElements {
        "cps-global-header": React.DetailedHTMLProps<
          React.HTMLAttributes<HTMLElement>,
          HTMLElement
        >;
      }
    }
  }
}
export {};
