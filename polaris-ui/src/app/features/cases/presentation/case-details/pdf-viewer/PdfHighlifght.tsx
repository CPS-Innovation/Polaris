import { Popup } from "../../../../../../react-pdf-highlighter";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { PdfAreaHighlight } from "./PdfAreaHighlight";
import { PdfLinearHighlight } from "./PdfLinearHighlight";
import { RemoveButton } from "./RemoveButton";
import { useEffect } from "react";
import { LTWH } from "../../../../../../react-pdf-highlighter/src/types";

type Props = {
  highlight: T_ViewportHighlight<IPdfHighlight>;
  index: number;
  setTip: (
    highlight: T_ViewportHighlight<IPdfHighlight>,
    callback: (highlight: T_ViewportHighlight<IPdfHighlight>) => JSX.Element
  ) => void;
  hideTip: () => void;
  isScrolledTo: boolean;
  handleRemoveRedaction: (id: string) => void;
  screenshot: (position: LTWH) => string;
  handleUpdateRedactionHighlight: (id: string, image: string) => void;
};

export const PdfHighlight: React.FC<Props> = ({
  highlight,
  index,
  setTip,
  hideTip,
  isScrolledTo,
  handleRemoveRedaction,
  handleUpdateRedactionHighlight,
  screenshot,
}) => {
  useEffect(() => {
    const image = screenshot(highlight.position.boundingRect);
    console.log("mounting pdfHighlight...", image);
    if (!highlight.image) {
      handleUpdateRedactionHighlight(highlight.id, image);
    }

    return () => {
      console.log("unmounting pdfHighlight...");
    };
  }, []);

  // console.log("highlight.image>>>>", highlight.image);
  const component =
    highlight.highlightType === "linear" ? (
      <PdfLinearHighlight
        type={highlight.type}
        isScrolledTo={isScrolledTo}
        highlight={highlight}
      />
    ) : (
      <PdfAreaHighlight isScrolledTo={isScrolledTo} highlight={highlight} />
    );

  return highlight.type === "search" ? (
    { ...component, key: index }
  ) : (
    <Popup
      popupContent={
        <RemoveButton
          onClick={() => {
            handleRemoveRedaction(highlight.id);
            hideTip();
          }}
        />
      }
      onMouseOver={(popupContent) =>
        setTip(highlight, (/*highlight*/) => popupContent)
      }
      onFocus={(popupContent) => {
        setTip(highlight, (/*highlight*/) => popupContent);
      }}
      onMouseOut={hideTip}
      key={index}
      children={component}
    />
  );
};
