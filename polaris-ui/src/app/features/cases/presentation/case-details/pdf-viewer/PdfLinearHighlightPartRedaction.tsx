import { LTWH } from "../../../../../../react-pdf-highlighter";
import { useRef } from "react";
import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
  id: string;
  textContent?: string;
  onClick?: () => {};
};

export const PdfLinearHighlightPartRedaction: React.FC<Props> = ({
  rect,
  id,
  textContent,
  onClick,
}) => {
  const buttonRef = useRef<HTMLButtonElement>(null);
  const handleFocus = () => {
    if (buttonRef.current) {
      buttonRef.current.scrollIntoView({
        behavior: "smooth",
        block: "center",
        inline: "nearest",
      });
    }
  };
  return (
    <>
      <button
        ref={buttonRef}
        aria-label="remove unsaved redaction"
        aria-describedby={`redacted-text-${id}`}
        style={rect}
        className={classes[`Highlight__part__redaction`]}
        data-testid={`unsaved-redaction-${id}`}
        onClick={onClick}
        onFocus={handleFocus}
      />
      <p
        hidden
        id={`redacted-text-${id}`}
      >{`The text redacted is, ${textContent}`}</p>
    </>
  );
};
