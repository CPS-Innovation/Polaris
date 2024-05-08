import { useEffect, useState } from "react";
import classes from "./LoaderUpdate.module.scss";

type Props = {
  textContent: string;
};

export const LoaderUpdate: React.FC<Props> = ({ textContent }) => {
  const [hideContent, setHideContent] = useState(false);
  useEffect(() => {
    setTimeout(() => {
      setHideContent(true);
    }, 100);
  }, []);
  return (
    <div
      aria-live="polite"
      className={classes.visuallyHidden}
      aria-hidden={hideContent}
    >
      {textContent}
    </div>
  );
};
