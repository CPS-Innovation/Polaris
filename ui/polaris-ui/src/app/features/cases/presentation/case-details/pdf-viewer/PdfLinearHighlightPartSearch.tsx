import { useCallback, useEffect, useState } from "react";
import { LTWH } from "../../../../../../react-pdf-highlighter";

import classes from "./PdfLinearHighlight.module.scss";

type Props = {
  rect: LTWH;
};

export const PdfLinearHighlightPartSearch: React.FC<Props> = ({ rect }) => {
  const [isHidden, setIsHidden] = useState(false);

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

  useEffect(() => {
    if (isHidden) {
      window.addEventListener("mousemove", handleReinstateHighlight);
    }
    return () => {
      window.removeEventListener("mousemove", handleReinstateHighlight);
    };
  }, [isHidden, handleReinstateHighlight]);

  return isHidden ? null : (
    <div
      style={rect}
      className={classes[`Highlight__part__search`]}
      onClick={() => setIsHidden(true)}
    />
  );
};
