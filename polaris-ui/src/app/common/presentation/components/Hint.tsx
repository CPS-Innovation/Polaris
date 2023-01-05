import * as GDS from "govuk-react-jsx";
import styles from "./Hint.module.scss";

type HintProps = React.DetailedHTMLProps<
  React.HTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> & {
  className?: string;
};

export const Hint: React.FC<HintProps> = ({
  className,
  children,
  ...attributes
}) => {
  return (
    <GDS.Hint className={`${styles.root} ${className}`} {...attributes}>
      {children}
    </GDS.Hint>
  );
};
