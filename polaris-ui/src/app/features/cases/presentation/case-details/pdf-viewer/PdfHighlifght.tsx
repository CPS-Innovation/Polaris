import { Popup } from "../../../../../../react-pdf-highlighter";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { ISearchPIIHighlight } from "../../../domain/NewPdfHighlight";
import { PdfAreaHighlight } from "./PdfAreaHighlight";
import { PdfLinearHighlight } from "./PdfLinearHighlight";
import { RemoveButton } from "./RemoveButton";

type Props = {
  highlight: T_ViewportHighlight<IPdfHighlight | ISearchPIIHighlight>;
  index: number;
  setTip: (
    highlight: T_ViewportHighlight<IPdfHighlight | ISearchPIIHighlight>,
    callback: (
      highlight: T_ViewportHighlight<IPdfHighlight | ISearchPIIHighlight>
    ) => JSX.Element
  ) => void;
  hideTip: () => void;
  isScrolledTo: boolean;
  handleRemoveRedaction: (id: string) => void;
  children?: React.ReactNode;
};

export const PdfHighlight: React.FC<Props> = ({
  highlight,
  index,
  setTip,
  hideTip,
  isScrolledTo,
  handleRemoveRedaction,
  children,
}) => {
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

  return highlight.type === "search" || highlight.type === "searchPII" ? (
    ({ ...component, key: index } as any)
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
      onClick={(popupContent) =>
        setTip(highlight, (/*highlight*/) => popupContent)
      }
      onMouseOut={hideTip}
      key={index}
      children={component}
    />
  );
};
