import * as GDS from "govuk-react-jsx";
import classes from "./Details.module.scss";
export type DetailsProps = React.DetailedHTMLProps<
  React.DetailsHTMLAttributes<HTMLDetailsElement>,
  HTMLDetailsElement
> & {
  className?: string;
  children?: React.ReactNode;
  summaryChildren?: React.ReactNode;
  isDefaultLeftBorderHidden?: boolean;
};

export const Details: React.FC<DetailsProps> = ({
  className,
  isDefaultLeftBorderHidden,
  ...restProps
}) => {
  const processedClassName = `${className} ${
    isDefaultLeftBorderHidden ? classes.disableDefaultDetailsText : ""
  }`;

  return (
    <GDS.Details className={processedClassName} {...restProps}></GDS.Details>
  );
};
