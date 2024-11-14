export const getDocument = (element) => element.ownerDocument || document;
export const getWindow = (element) => getDocument(element).defaultView || window;
export const isHTMLElement = (element) => element != null &&
    (element instanceof HTMLElement ||
        element instanceof getWindow(element).HTMLElement);
export const isHTMLCanvasElement = (element) => element instanceof HTMLCanvasElement ||
    element instanceof getWindow(element).HTMLCanvasElement;
export const getPageFromElement = (target) => {
    const node = target.closest(".page");
    if (!isHTMLElement(node)) {
        return null;
    }
    const number = Number(node.dataset.pageNumber);
    return { node, number };
};
export const getPagesFromRange = (range) => {
    const startParentElement = range.startContainer.parentElement;
    const endParentElement = range.endContainer.parentElement;
    if (!isHTMLElement(startParentElement) || !isHTMLElement(endParentElement)) {
        return [];
    }
    const startPage = getPageFromElement(startParentElement);
    const endPage = getPageFromElement(endParentElement);
    if (!(startPage === null || startPage === void 0 ? void 0 : startPage.number) || !(endPage === null || endPage === void 0 ? void 0 : endPage.number)) {
        return [];
    }
    if (startPage.number === endPage.number) {
        return [startPage];
    }
    if (startPage.number === endPage.number - 1) {
        return [startPage, endPage];
    }
    const pages = [];
    let currentPageNumber = startPage.number;
    const document = startPage.node.ownerDocument;
    while (currentPageNumber <= endPage.number) {
        const currentPage = getPageFromElement(document.querySelector(`[data-page-number='${currentPageNumber}'`));
        if (currentPage) {
            pages.push(currentPage);
        }
        currentPageNumber++;
    }
    return pages;
};
export const findOrCreateContainerLayer = (container, className) => {
    const doc = getDocument(container);
    let layer = container.querySelector(`.${className}`);
    if (!layer) {
        layer = doc.createElement("div");
        layer.className = className;
        container.appendChild(layer);
    }
    return layer;
};
//# sourceMappingURL=pdfjs-dom.js.map