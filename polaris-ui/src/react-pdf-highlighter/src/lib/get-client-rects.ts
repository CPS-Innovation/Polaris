import type { LTWHP, Page } from "../types.js";

import optimizeClientRects from "./optimize-client-rects";

const isClientRectInsidePageRect = (
  clientRect: DOMRect,
  pageRect: DOMRect,
  borderLeftWidth: number,
  borderTopWidth: number
) => {
  if (clientRect.top < pageRect.top + borderTopWidth) {
    return false;
  }
  if (clientRect.bottom > pageRect.bottom - borderTopWidth) {
    return false;
  }
  if (clientRect.right > pageRect.right - borderLeftWidth) {
    return false;
  }
  if (clientRect.left < pageRect.left + borderLeftWidth) {
    return false;
  }

  return true;
};

const getClientRects = (
  range: Range,
  pages: Page[],
  shouldOptimize = true
): Array<LTWHP> => {
  const clientRects = Array.from(range.getClientRects());

  const rects: LTWHP[] = [];

  for (const clientRect of clientRects) {
    for (const page of pages) {
      const pageRect = page.node.getBoundingClientRect();
      const borderLeftWidth = page.node.clientLeft;
      const borderTopWidth = page.node.clientTop;

      if (
        isClientRectInsidePageRect(
          clientRect,
          pageRect,
          borderLeftWidth,
          borderTopWidth
        ) &&
        clientRect.width > 0 &&
        clientRect.height > 0 &&
        clientRect.width < pageRect.width &&
        clientRect.height < pageRect.height
      ) {
        const highlightedRect = {
          top:
            clientRect.top +
            page.node.scrollTop -
            pageRect.top -
            borderTopWidth,
          left:
            clientRect.left +
            page.node.scrollLeft -
            pageRect.left -
            borderLeftWidth,
          width: clientRect.width,
          height: clientRect.height,
          pageNumber: page.number,
        } as LTWHP;

        rects.push(highlightedRect);
      }
    }
  }

  return shouldOptimize ? optimizeClientRects(rects) : rects;
};

export default getClientRects;
