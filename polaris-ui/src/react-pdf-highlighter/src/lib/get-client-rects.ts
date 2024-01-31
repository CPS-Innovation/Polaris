import type { LTWHP, Page } from "../types.js";

import optimizeClientRects from "./optimize-client-rects";

export const PAGE_BORDER_WIDTH = 9;

const isClientRectInsidePageRect = (clientRect: DOMRect, pageRect: DOMRect) => {
  if (clientRect.top < pageRect.top + PAGE_BORDER_WIDTH) {
    return false;
  }
  if (clientRect.bottom > pageRect.bottom - PAGE_BORDER_WIDTH) {
    return false;
  }
  if (clientRect.right > pageRect.right - PAGE_BORDER_WIDTH) {
    return false;
  }
  if (clientRect.left < pageRect.left + PAGE_BORDER_WIDTH) {
    return false;
  }

  return true;
};

const getClientRects = (
  range: Range,
  pages: Page[],
  shouldOptimize: boolean = true
): Array<LTWHP> => {
  const clientRects = Array.from(range.getClientRects());

  const rects: LTWHP[] = [];

  for (const clientRect of clientRects) {
    for (const page of pages) {
      const pageRect = page.node.getBoundingClientRect();

      if (
        isClientRectInsidePageRect(clientRect, pageRect) &&
        clientRect.top >= 0 &&
        clientRect.bottom >= 0 &&
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
            PAGE_BORDER_WIDTH,
          left:
            clientRect.left +
            page.node.scrollLeft -
            pageRect.left -
            PAGE_BORDER_WIDTH,
          width: clientRect.width,
          height: clientRect.height,
          pageNumber: page.number,
        } as LTWHP;

        rects.push(highlightedRect);
      }
    }
  }

  console.log("rects>>", rects);

  return shouldOptimize ? optimizeClientRects(rects) : rects;
};

export default getClientRects;
