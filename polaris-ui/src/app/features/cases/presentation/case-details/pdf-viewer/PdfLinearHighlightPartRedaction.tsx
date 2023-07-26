import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
  redactionAddedOrder?: number;
  textContent?: string;
};

export const PdfLinearHighlightPartRedaction: React.FC<Props> = ({
  rect,
  redactionAddedOrder,
  textContent = "",
}) => {
  return (
    <>
      <button
        aria-label="unsaved redaction button"
        aria-describedby={`redacted-text-${redactionAddedOrder}`}
        style={rect}
        className={classes[`Highlight__part__redaction`]}
        data-redaction-added-order={redactionAddedOrder}
      />
      <p
        hidden
        id={`redacted-text-${redactionAddedOrder}`}
      >{`The text redacted is, ${textContent}`}</p>
    </>
  );
};
