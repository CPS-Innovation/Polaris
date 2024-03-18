import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
  id: string;
  textContent?: string;
};

export const PdfLinearHighlightPartRedaction: React.FC<Props> = ({
  rect,
  id,
  textContent,
}) => {
  return (
    <>
      <button
        aria-label="unsaved redaction"
        aria-describedby={`redacted-text-${id}`}
        style={rect}
        className={classes[`Highlight__part__redaction`]}
        data-testid={`unsaved-redaction-${id}`}
      />
      <p
        hidden
        id={`redacted-text-${id}`}
      >{`The text redacted is, ${textContent}`}</p>
    </>
  );
};
