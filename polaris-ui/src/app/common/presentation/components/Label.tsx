import * as GDS from "govuk-react-jsx";

export type LabelProps = React.DetailedHTMLProps<
  React.LabelHTMLAttributes<HTMLLabelElement>,
  HTMLLabelElement
> & {
  className?: string;
  htmlFor?: string;
  children?: React.ReactNode;
  isPageHeading?: boolean;
};

export const Label: React.FC<LabelProps> = (props) => {
  return <GDS.Label {...props}></GDS.Label>;
};
