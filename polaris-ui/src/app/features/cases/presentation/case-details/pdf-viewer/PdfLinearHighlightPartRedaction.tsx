import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
};

export const PdfLinearHighlightPartRedaction: React.FC<Props> = ({ rect }) => {
  return (
    <button style={rect} className={classes[`Highlight__part__redaction`]} />
  );
};
