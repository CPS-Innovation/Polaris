import React from "react";
import { viewportToScaled } from "../lib/coordinates";
export function HighlightLayer({ highlightsByPage, scaledPositionToViewport, pageNumber, scrolledToHighlightId, highlightTransform, tip, hideTipAndSelection, viewer, screenshot, showTip, setTip, }) {
    const currentHighlights = highlightsByPage[String(pageNumber)] || [];
    return (React.createElement("div", null, currentHighlights.map((highlight, index) => {
        const viewportHighlight = Object.assign(Object.assign({}, highlight), { position: scaledPositionToViewport(highlight.position) });
        if (tip && tip.highlight.id === String(highlight.id)) {
            showTip(tip.highlight, tip.callback(viewportHighlight));
        }
        const isScrolledTo = Boolean(scrolledToHighlightId === highlight.id);
        return highlightTransform(viewportHighlight, index, (highlight, callback) => {
            setTip({ highlight, callback });
            showTip(highlight, callback(highlight));
        }, hideTipAndSelection, (rect) => {
            const viewport = viewer.getPageView((rect.pageNumber || Number.parseInt(pageNumber)) - 1).viewport;
            return viewportToScaled(rect, viewport);
        }, (boundingRect) => screenshot(boundingRect, Number.parseInt(pageNumber)), isScrolledTo);
    })));
}
//# sourceMappingURL=HighlightLayer.js.map