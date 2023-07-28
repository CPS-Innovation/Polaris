import { Popup } from "../../../../../../react-pdf-highlighter";
import { T_ViewportHighlight } from "../../../../../../react-pdf-highlighter/src/components/PdfHighlighter";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { PdfAreaHighlight } from "./PdfAreaHighlight";
import { PdfLinearHighlight } from "./PdfLinearHighlight";
import { RemoveButton } from "./RemoveButton";

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
};

export const PdfHighlight: React.FC<Props> = ({
  highlight,
  index,
  setTip,
  hideTip,
  isScrolledTo,
  handleRemoveRedaction,
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
