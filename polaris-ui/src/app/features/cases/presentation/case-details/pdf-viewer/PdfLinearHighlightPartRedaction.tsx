import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
  redactionAddedOrder?: number;
};

export const PdfLinearHighlightPartRedaction: React.FC<Props> = ({
  rect,
  redactionAddedOrder,
}) => {
  return (
    <button
      style={rect}
      className={classes[`Highlight__part__redaction`]}
      data-redaction-added-order={redactionAddedOrder}
    />
  );
};
