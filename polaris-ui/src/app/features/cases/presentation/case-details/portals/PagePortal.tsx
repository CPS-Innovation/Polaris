import React, {
  useCallback,
  useEffect,
  useRef,
  useState,
  ReactElement,
} from "react";
import ReactDOM from "react-dom";
type PagePortalProps = {
  tabIndex: number;
  children: ReactElement;
};

export const PagePortal: React.FC<PagePortalProps> = ({
  tabIndex,
  children,
}) => {
  const portalNodeRefs = useRef<HTMLDivElement[]>([]);
  const [pageElements, setPageElements] = useState<Element[]>([]);
  const resizeObserverRef = useRef<ResizeObserver | null>(null);
  const mutationObserversRef = useRef<MutationObserver[]>([]);
  const isUnmountedRef = useRef(false);

  const updatePortals = useCallback(() => {
    const pdfViewer = document?.querySelectorAll(".pdfViewer")?.[tabIndex];
    if (!pdfViewer) {
      return;
    }
    const updatedPages = Array.from(pdfViewer.querySelectorAll(".page"));
    // Cleanup old portal nodes
    portalNodeRefs.current.forEach((portalDiv) => {
      if (portalDiv?.parentNode) {
        portalDiv.parentNode.removeChild(portalDiv);
      }
    });

    // Attach new portal nodes to updated .page elements
    portalNodeRefs.current = updatedPages.map((pageDiv) => {
      const portalDiv = document.createElement("div");
      pageDiv.appendChild(portalDiv);
      return portalDiv;
    });
    setPageElements(updatedPages);
  }, [tabIndex]);

  //adding mutation observer for the pages to identify the  change data-loaded property to rerender portal
  const observePageMutations = useCallback(
    (pageElements: Element[]) => {
      mutationObserversRef.current.forEach((observer) => observer.disconnect());
      mutationObserversRef.current = [];
      pageElements.forEach((pageDiv, index) => {
        const observer = new MutationObserver(() => {
          updatePortals();
        });
        observer.observe(pageDiv, {
          childList: false,
          subtree: false,
          attributes: true,
          attributeFilter: ["data-loaded"],
        });
        mutationObserversRef.current.push(observer);
      });
    },
    [updatePortals]
  );

  useEffect(() => {
    //add the page mutation observer when pages are available
    if (!mutationObserversRef.current.length && pageElements.length) {
      observePageMutations(pageElements);
    }
  }, [pageElements, observePageMutations]);

  useEffect(() => {
    if (isUnmountedRef.current) return;
    const pdfViewer = document?.querySelectorAll(".pdfViewer")[tabIndex];
    resizeObserverRef.current = new ResizeObserver((entries) => {
      updatePortals();
    });
    resizeObserverRef.current?.observe(pdfViewer);

    return () => {
      // Cleanup, disconnect observer and portal nodes
      if (resizeObserverRef.current) {
        resizeObserverRef.current.disconnect();
        resizeObserverRef.current = null;
      }
      mutationObserversRef.current.forEach((observer) => observer.disconnect());
      mutationObserversRef.current = [];

      portalNodeRefs.current.forEach((portalDiv) => {
        if (portalDiv?.parentNode) {
          portalDiv.parentNode.removeChild(portalDiv);
        }
      });
      portalNodeRefs.current = [];
      isUnmountedRef.current = true;
    };
  }, []);

  return (
    <>
      {portalNodeRefs.current.map((portalNode, index) =>
        ReactDOM.createPortal(
          <div key={index}>
            {React.cloneElement(children, {
              pageNumber: index + 1,
              totalPages: portalNodeRefs.current.length,
            })}
          </div>,
          portalNode
        )
      )}
    </>
  );
};