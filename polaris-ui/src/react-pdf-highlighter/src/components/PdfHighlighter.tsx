import "pdfjs-dist/web/pdf_viewer.css";
import "../style/pdf_viewer.css";
import "../style/PdfHighlighter.css";

import debounce from "lodash.debounce";
import type { PDFDocumentProxy } from "pdfjs-dist";
import type { EventBus, PDFViewer } from "pdfjs-dist/web/pdf_viewer.mjs";
import React, {
  type PointerEventHandler,
  PureComponent,
  type RefObject,
} from "react";
import { type Root, createRoot } from "react-dom/client";
import { scaledToViewport, viewportToScaled } from "../lib/coordinates";
import getAreaAsPng from "../lib/get-area-as-png";
import getBoundingRect from "../lib/get-bounding-rect";
import getClientRects, { PAGE_BORDER_WIDTH } from "../lib/get-client-rects";
import {
  findOrCreateContainerLayer,
  getPageFromElement,
  getPagesFromRange,
  getWindow,
  isHTMLElement,
} from "../lib/pdfjs-dom";
import type {
  IHighlight,
  LTWH,
  LTWHP,
  Position,
  Scaled,
  ScaledPosition,
} from "../types";
import { HighlightLayer } from "./HighlightLayer";
import MouseSelection from "./MouseSelection";
import TipContainer from "./TipContainer";

export type T_ViewportHighlight<T_HT> = { position: Position } & T_HT;

interface State<T_HT> {
  ghostHighlight: {
    position: ScaledPosition;
    content?: { text?: string; image?: string };
  } | null;
  isCollapsed: boolean;
  range: Range | null;
  tip: {
    highlight: T_ViewportHighlight<T_HT>;
    callback: (highlight: T_ViewportHighlight<T_HT>) => JSX.Element;
  } | null;
  tipPosition: Position | null;
  tipChildren: JSX.Element | null;
  isAreaSelectionInProgress: boolean;
  scrolledToHighlightId: string;
}

interface Props<T_HT> {
  highlightTransform: (
    highlight: T_ViewportHighlight<T_HT>,
    index: number,
    setTip: (
      highlight: T_ViewportHighlight<T_HT>,
      callback: (highlight: T_ViewportHighlight<T_HT>) => JSX.Element
    ) => void,
    hideTip: () => void,
    viewportToScaled: (rect: LTWHP) => Scaled,
    screenshot: (position: LTWH) => string,
    isScrolledTo: boolean
  ) => JSX.Element;
  highlights: Array<T_HT>;
  onScrollChange: () => void;
  scrollRef: (scrollTo: (highlight: IHighlight) => void) => void;
  pdfDocument: PDFDocumentProxy;
  pdfScaleValue: string;
  onSelectionFinished: (
    position: ScaledPosition,
    content: {
      text?: string;
      image?: string;
      highlightType?: "search" | "searchPII" | "none";
      highlightGroupId?: string;
    },
    hideTipAndSelection: () => void,
    transformSelection: () => void
  ) => JSX.Element | null;
  enableAreaSelection: (event: MouseEvent) => boolean;
  onWheelDownwards?: () => void;
}

const EMPTY_ID = "empty-id";

export class PdfHighlighter<T_HT extends IHighlight> extends PureComponent<
  Props<T_HT>,
  State<T_HT>
> {
  static defaultProps = {
    pdfScaleValue: "auto",
  };

  state: State<T_HT> = {
    ghostHighlight: null,
    isCollapsed: true,
    range: null,
    scrolledToHighlightId: EMPTY_ID,
    isAreaSelectionInProgress: false,
    tip: null,
    tipPosition: null,
    tipChildren: null,
  };

  viewer!: PDFViewer;

  resizeObserver: ResizeObserver | null = null;
  containerNode?: HTMLDivElement | null = null;
  containerNodeRef: RefObject<HTMLDivElement>;
  highlightRoots: {
    [page: number]: { reactRoot: Root; container: Element };
  } = {};
  unsubscribe = () => {
    // No-op by default, can be overridden
  };
  mouseSelectionRef: React.RefObject<MouseSelection>;
  constructor(props: Props<T_HT>) {
    super(props);
    // if (typeof ResizeObserver !== "undefined") {
    //   this.resizeObserver = new ResizeObserver(this.debouncedScaleValue);
    // }
    this.containerNodeRef = React.createRef();
    this.mouseSelectionRef = React.createRef();
  }

  /***
   * This is to clear up the area selection and hide tip if the user clicks outside the document
   * clearing up of area selection, if the user clicks inside the document is already done MouseSelection component.
   */
  documentClickHandler = (event: MouseEvent) => {
    const { tipPosition } = this.state;
    //only consider hide tip if there is one
    if (!tipPosition) {
      return;
    }
    if ((event.target as HTMLElement).closest(".PdfHighlighter")) {
      return;
    }
    if (
      (event.target as HTMLElement).closest(".PdfHighlighter__tip-container")
    ) {
      return;
    }
    if ((event.target as HTMLElement)?.classList.contains("PdfHighlighter")) {
      return;
    }

    this.hideTipAndSelection();
    this.mouseSelectionRef.current?.reset();
  };

  componentDidMount() {
    this.init();
    document.addEventListener("click", this.documentClickHandler);
  }

  attachRef = (eventBus: EventBus) => {
    const { resizeObserver: observer } = this;
    this.containerNode = this.containerNodeRef.current;
    this.unsubscribe();

    if (this.containerNode) {
      const { ownerDocument: doc } = this.containerNode;
      eventBus.on("textlayerrendered", this.onTextLayerRendered);
      eventBus.on("pagesinit", this.onDocumentReady);
      doc.addEventListener("selectionchange", this.onSelectionChange);
      doc.addEventListener("keydown", this.handleKeyDown);
      doc.defaultView?.addEventListener(
        "visibilitychange",
        this.debouncedScaleValue
      );
      if (observer) observer.observe(this.containerNode);
      this.containerNode.addEventListener("wheel", this.handleWheel, {
        passive: true,
      });
      this.unsubscribe = () => {
        eventBus.off("pagesinit", this.onDocumentReady);
        eventBus.off("textlayerrendered", this.onTextLayerRendered);
        doc.removeEventListener("selectionchange", this.onSelectionChange);
        doc.removeEventListener("keydown", this.handleKeyDown);
        doc.defaultView?.removeEventListener(
          "visibilitychange",
          this.debouncedScaleValue
        );
        if (observer) observer.disconnect();
        if (this.containerNode)
          this.containerNode.removeEventListener("wheel", this.handleWheel);
      };
    }
  };
  handleWheel = (ev: WheelEvent) => {
    if (this.props.onWheelDownwards && ev.deltaY > 0) {
      //ev.preventDefault();
      this.props.onWheelDownwards();
    }
  };

  componentDidUpdate(prevProps: Props<T_HT>) {
    if (prevProps.pdfDocument !== this.props.pdfDocument) {
      this.init();
      return;
    }
    if (prevProps.highlights !== this.props.highlights) {
      this.renderHighlightLayers();
    }
  }

  async init() {
    const { pdfDocument } = this.props;
    const pdfjs = await import("pdfjs-dist/web/pdf_viewer.mjs");

    const eventBus = new pdfjs.EventBus();
    const linkService = new pdfjs.PDFLinkService({
      eventBus,
      externalLinkTarget: 2,
    });

    if (!this.containerNodeRef.current) {
      throw new Error("!");
    }

    this.viewer =
      this.viewer ||
      new pdfjs.PDFViewer({
        container: this.containerNodeRef.current,
        eventBus: eventBus,
        // enhanceTextSelection: true, // deprecated. https://github.com/mozilla/pdf.js/issues/9943#issuecomment-409369485
        textLayerMode: 2,
        // removePageBorders: true,
        linkService: linkService,
      });

    linkService.setDocument(pdfDocument);
    linkService.setViewer(this.viewer);
    this.viewer.setDocument(pdfDocument);

    this.attachRef(eventBus);
  }

  componentWillUnmount() {
    this.unsubscribe();
    document.removeEventListener("click", this.documentClickHandler);
  }

  findOrCreateHighlightLayer(page: number) {
    const pageView = this.viewer.getPageView(page - 1) || {};
    const { textLayer } = pageView;

    if (!textLayer) {
      return null;
    }

    return findOrCreateContainerLayer(
      textLayer.div,
      "PdfHighlighter__highlight-layer"
    );
  }

  groupHighlightsByPage(highlights: Array<T_HT>): {
    [pageNumber: string]: Array<T_HT>;
  } {
    const { ghostHighlight } = this.state;

    const allHighlights = [...highlights, ghostHighlight].filter(
      Boolean
    ) as T_HT[];

    const pageNumbers = new Set<number>();
    for (const highlight of allHighlights) {
      pageNumbers.add(highlight.position.pageNumber);
      for (const rect of highlight.position.rects) {
        if (rect.pageNumber) {
          pageNumbers.add(rect.pageNumber);
        }
      }
    }

    const groupedHighlights: Record<number, T_HT[]> = {};

    for (const pageNumber of pageNumbers) {
      groupedHighlights[pageNumber] = groupedHighlights[pageNumber] || [];
      for (const highlight of allHighlights) {
        const pageSpecificHighlight = {
          ...highlight,
          position: {
            pageNumber,
            boundingRect: highlight.position.boundingRect,
            rects: [],
            usePdfCoordinates: highlight.position.usePdfCoordinates,
          } as ScaledPosition,
        };
        let anyRectsOnPage = false;
        for (const rect of highlight.position.rects) {
          if (
            pageNumber === (rect.pageNumber || highlight.position.pageNumber)
          ) {
            pageSpecificHighlight.position.rects.push(rect);
            anyRectsOnPage = true;
          }
        }
        if (anyRectsOnPage || pageNumber === highlight.position.pageNumber) {
          groupedHighlights[pageNumber].push(pageSpecificHighlight);
        }
      }
    }

    return groupedHighlights;
  }

  showTip(highlight: T_ViewportHighlight<T_HT>, content: JSX.Element) {
    const { isCollapsed, ghostHighlight, isAreaSelectionInProgress } =
      this.state;

    const highlightInProgress = !isCollapsed || ghostHighlight;

    if (highlightInProgress || isAreaSelectionInProgress) {
      return;
    }

    this.setTip(highlight.position, content);
  }

  scaledPositionToViewport({
    pageNumber,
    boundingRect,
    rects,
    usePdfCoordinates,
  }: ScaledPosition): Position {
    const viewport = this.viewer.getPageView(pageNumber - 1).viewport;

    return {
      boundingRect: scaledToViewport(boundingRect, viewport, usePdfCoordinates),
      rects: (rects || []).map((rect) =>
        scaledToViewport(rect, viewport, usePdfCoordinates)
      ),
      pageNumber,
    };
  }

  viewportPositionToScaled({
    pageNumber,
    boundingRect,
    rects,
  }: Position): ScaledPosition {
    const viewport = this.viewer.getPageView(pageNumber - 1).viewport;

    return {
      boundingRect: viewportToScaled(boundingRect, viewport),
      rects: (rects || []).map((rect) => viewportToScaled(rect, viewport)),
      pageNumber,
    };
  }

  screenshot(position: LTWH, pageNumber: number) {
    const canvas = this.viewer.getPageView(pageNumber - 1).canvas;

    return getAreaAsPng(canvas, position);
  }
  private renderHighlightLayers() {
    const { pdfDocument } = this.props;
    for (let pageNumber = 1; pageNumber <= pdfDocument.numPages; pageNumber++) {
      const highlightRoot = this.highlightRoots[pageNumber];
      // /** Need to check if container is still attached to the DOM as PDF.js can unload pages. */
      if (highlightRoot?.container.isConnected) {
        this.renderHighlightLayer(highlightRoot.reactRoot, pageNumber);
      } else {
        const highlightLayer = this.findOrCreateHighlightLayer(pageNumber);
        if (highlightLayer) {
          const reactRoot = createRoot(highlightLayer);
          this.highlightRoots[pageNumber] = {
            reactRoot,
            container: highlightLayer,
          };
          this.renderHighlightLayer(reactRoot, pageNumber);
        }
      }
    }
  }

  private renderHighlightLayer(root: Root, pageNumber: number) {
    const { highlightTransform, highlights } = this.props;
    const { tip, scrolledToHighlightId } = this.state;

    root.render(
      <HighlightLayer
        highlightsByPage={this.groupHighlightsByPage(highlights)}
        pageNumber={pageNumber.toString()}
        scrolledToHighlightId={scrolledToHighlightId}
        highlightTransform={highlightTransform}
        tip={tip}
        scaledPositionToViewport={this.scaledPositionToViewport.bind(this)}
        hideTipAndSelection={this.hideTipAndSelection.bind(this)}
        viewer={this.viewer}
        screenshot={this.screenshot.bind(this)}
        showTip={this.showTip.bind(this)}
        setTip={(tip) => {
          this.setState({ tip });
        }}
      />
    );
  }

  hideTipAndSelection = () => {
    this.setState({
      tipPosition: null,
      tipChildren: null,
    });

    this.setState({ ghostHighlight: null, tip: null, isCollapsed: true }, () =>
      this.renderHighlightLayers()
    );
  };

  setTip(position: Position, inner: JSX.Element | null) {
    this.setState({
      tipPosition: position,
      tipChildren: inner,
    });
  }

  renderTip = () => {
    const { tipPosition, tipChildren } = this.state;
    if (!tipPosition) return null;

    const { boundingRect, pageNumber } = tipPosition;
    const page = {
      node: this.viewer.getPageView((boundingRect.pageNumber || pageNumber) - 1)
        .div,
      pageNumber: boundingRect.pageNumber || pageNumber,
    };

    const pageBoundingClientRect = page.node.getBoundingClientRect();

    const pageBoundingRect = {
      bottom: pageBoundingClientRect.bottom,
      height: pageBoundingClientRect.height,
      left: pageBoundingClientRect.left,
      right: pageBoundingClientRect.right,
      top: pageBoundingClientRect.top,
      width: pageBoundingClientRect.width,
      x: pageBoundingClientRect.x,
      y: pageBoundingClientRect.y,
      pageNumber: page.pageNumber,
    };

    return (
      <TipContainer
        scrollTop={this.viewer.container.scrollTop}
        pageBoundingRect={pageBoundingRect}
        style={{
          left:
            page.node.offsetLeft + boundingRect.left + boundingRect.width / 2,
          top: boundingRect.top + page.node.offsetTop,
          bottom: boundingRect.top + page.node.offsetTop + boundingRect.height,
        }}
      >
        {tipChildren}
      </TipContainer>
    );
  };

  onTextLayerRendered = () => {
    this.renderHighlightLayers();
  };

  scrollTo = (highlight: IHighlight) => {
    const { pageNumber, boundingRect, usePdfCoordinates } = highlight.position;

    this.viewer.container.removeEventListener("scroll", this.onScroll);

    const pageViewport = this.viewer.getPageView(pageNumber - 1).viewport;

    const scrollMargin = 10;

    this.viewer.scrollPageIntoView({
      pageNumber,
      destArray: [
        null,
        { name: "XYZ" },
        ...pageViewport.convertToPdfPoint(
          0,
          scaledToViewport(boundingRect, pageViewport, usePdfCoordinates).top -
            scrollMargin
        ),
        0,
      ],
    });

    this.setState(
      {
        scrolledToHighlightId: highlight.id,
      },
      () => this.renderHighlightLayers()
    );

    // wait for scrolling to finish
    setTimeout(() => {
      this.viewer.container.addEventListener("scroll", this.onScroll);
    }, 100);
  };

  onDocumentReady = () => {
    const { scrollRef } = this.props;

    this.handleScaleValue();

    scrollRef(this.scrollTo);
  };

  onSelectionChange = () => {
    const container = this.containerNode;
    if (!container) {
      return;
    }

    const selection = getWindow(container).getSelection();
    if (!selection) {
      return;
    }

    const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;

    if (selection.isCollapsed) {
      this.setState({ isCollapsed: true });
      return;
    }

    if (
      !range ||
      !container ||
      !container.contains(range.commonAncestorContainer)
    ) {
      return;
    }

    this.setState({
      isCollapsed: false,
      range,
    });

    this.debouncedAfterSelection();
  };

  onScroll = () => {
    const { onScrollChange } = this.props;

    onScrollChange();

    this.setState(
      {
        scrolledToHighlightId: EMPTY_ID,
      },
      () => this.renderHighlightLayers()
    );

    this.viewer.container.removeEventListener("scroll", this.onScroll);
  };

  onMouseDown: PointerEventHandler = (event) => {
    if (!(event.target instanceof Element) || !isHTMLElement(event.target)) {
      return;
    }

    if (event.target.closest(".PdfHighlighter__tip-container")) {
      return;
    }

    this.hideTipAndSelection();
  };

  handleKeyDown = (event: KeyboardEvent) => {
    if (event.code === "Escape") {
      this.hideTipAndSelection();
    }
  };

  afterSelection = () => {
    const { onSelectionFinished } = this.props;

    const { isCollapsed, range } = this.state;

    if (!range || isCollapsed) {
      return;
    }

    const pages = getPagesFromRange(range);

    if (!pages || pages.length === 0) {
      return;
    }

    const rects = getClientRects(range, pages);

    if (rects.length === 0) {
      return;
    }
    //for multi-page text selection only handle the starting page redaction
    const samePageRects = rects.filter(
      (rect) => rect?.pageNumber === pages[0].number
    );

    if (samePageRects.length === 0) {
      return;
    }

    const boundingRect = getBoundingRect(samePageRects);

    const viewportPosition: Position = {
      boundingRect,
      rects: samePageRects,
      pageNumber: pages[0].number,
    };

    let content: {
      text: string;
      highlightType: "none" | "searchPII" | "search";
      highlightGroupId: string;
    } = {
      text: "",
      highlightType: "none",
      highlightGroupId: "",
    };
    if (range.toString()) {
      content.text = range.toString();
    } else {
      const attributes: any = (range.startContainer as HTMLElement)?.attributes;
      const textContent = attributes?.["text-content"]?.nodeValue;
      const highlightType = attributes?.["highlight-type"]?.nodeValue;
      const highlightGroupId = attributes?.["highlight-groupid"]?.nodeValue;
      content = {
        text: textContent ?? "",
        highlightType: highlightType,
        highlightGroupId: highlightGroupId,
      };
    }
    const scaledPosition = this.viewportPositionToScaled(viewportPosition);

    this.setTip(
      viewportPosition,
      onSelectionFinished(
        scaledPosition,
        content,
        () => this.hideTipAndSelection(),
        () =>
          this.setState(
            {
              ghostHighlight: { position: scaledPosition },
            },
            () => this.renderHighlightLayers()
          )
      )
    );
  };

  debouncedAfterSelection: () => void = debounce(this.afterSelection, 500);

  toggleTextSelection(flag: boolean) {
    if (!this.viewer.viewer) {
      return;
    }
    this.viewer.viewer.classList.toggle(
      "PdfHighlighter--disable-selection",
      flag
    );
  }

  handleScaleValue = () => {
    if (this.viewer) {
      this.viewer.currentScaleValue = this.props.pdfScaleValue; //"page-width";
    }
  };

  debouncedScaleValue: () => void = debounce(this.handleScaleValue, 500);
  shouldStart = (event: MouseEvent) => {
    const { enableAreaSelection } = this.props;

    return (
      enableAreaSelection(event) &&
      isHTMLElement(event.target as HTMLElement) &&
      !!(event.target as HTMLElement).closest(".page") &&
      !(event.target as HTMLElement).closest(".page-portal") //if the target is inside pagePortal ignore it from area selection handling
    );
  };
  render() {
    const { onSelectionFinished, enableAreaSelection } = this.props;

    return (
      <div onPointerDown={this.onMouseDown}>
        <div ref={this.containerNodeRef} className="PdfHighlighter">
          <div className="pdfViewer" />
          {this.renderTip()}
          {typeof enableAreaSelection === "function" ? (
            <MouseSelection
              ref={this.mouseSelectionRef}
              onDragStart={() => this.toggleTextSelection(true)}
              onDragEnd={() => this.toggleTextSelection(false)}
              onChange={(isVisible) =>
                this.setState({ isAreaSelectionInProgress: isVisible })
              }
              shouldStart={this.shouldStart}
              onSelection={(startTarget, boundingRect, resetSelection) => {
                const page = getPageFromElement(startTarget);

                if (!page) {
                  return;
                }

                const pageBoundingRect = {
                  ...boundingRect,
                  top:
                    boundingRect.top - page.node.offsetTop - PAGE_BORDER_WIDTH,
                  left:
                    boundingRect.left -
                    page.node.offsetLeft -
                    PAGE_BORDER_WIDTH,
                  pageNumber: page.number,
                };

                const viewportPosition = {
                  boundingRect: pageBoundingRect,
                  rects: [],
                  pageNumber: page.number,
                };

                const scaledPosition =
                  this.viewportPositionToScaled(viewportPosition);

                const image = this.screenshot(
                  pageBoundingRect,
                  pageBoundingRect.pageNumber
                );

                this.setTip(
                  viewportPosition,
                  onSelectionFinished(
                    scaledPosition,
                    { image },
                    () => this.hideTipAndSelection(),
                    () => {
                      this.setState(
                        {
                          ghostHighlight: {
                            position: scaledPosition,
                            content: { image },
                          },
                        },
                        () => {
                          resetSelection();
                          this.renderHighlightLayers();
                        }
                      );
                    }
                  )
                );
              }}
            />
          ) : null}
        </div>
      </div>
    );
  }
}
