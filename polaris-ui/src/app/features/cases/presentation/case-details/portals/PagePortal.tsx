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

  useEffect(() => {
    setTimeout(() => {
      if (isUnmountedRef.current) return;
      const pdfViewer = document?.querySelectorAll(".pdfViewer")[tabIndex];
      resizeObserverRef.current = new ResizeObserver((entries) => {
        updatePortals();
      });
      resizeObserverRef.current?.observe(pdfViewer);
    }, 1000);

    return () => {
      // Cleanup, disconnect observer and portal nodes
      if (resizeObserverRef.current) {
        resizeObserverRef.current.disconnect();
      }
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
