import { useCallback, useEffect, useState, useRef } from "react";
import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
};

export const PdfLinearHighlightPartSearch: React.FC<Props> = ({ rect }) => {
  const [isHidden, setIsHidden] = useState(false);
  const ref = useRef<HTMLButtonElement>(null);
  const handleReinstateHighlight = useCallback(
    (event: MouseEvent) => {
      const { offsetX, offsetY } = event;
      const { top, left, width, height } = rect;
      const isWithinX = offsetX > left && offsetX < left + width;
      const isWithinY = offsetY > top && offsetY < top + height;

      if (!(isWithinX && isWithinY)) {
        window.removeEventListener("mousemove", handleReinstateHighlight);
        setIsHidden(false);
      }
    },
    [rect]
  );

  const triggerSelectionChangeForRedaction = () => {
    if (ref?.current) {
      window.getSelection()?.removeAllRanges();
      const range = document.createRange();
      range.selectNodeContents(ref?.current.parentNode!);
      window.getSelection()?.addRange(range);

      const selectionChangeEvent = new Event("selectionchange");
      document.dispatchEvent(selectionChangeEvent);
    }
  };

  useEffect(() => {
    if (isHidden) {
      window.addEventListener("mousemove", handleReinstateHighlight);
    }
    return () => {
      window.removeEventListener("mousemove", handleReinstateHighlight);
    };
  }, [isHidden, handleReinstateHighlight]);

  return isHidden ? null : (
    <button
      ref={ref}
      style={rect}
      className={classes[`Highlight__part__search`]}
      onClick={triggerSelectionChangeForRedaction}
    />
  );
};
