import * as GDS from "govuk-react-jsx";
import React, { ReactNode } from "react";

type ErrorSummaryProps = React.DetailedHTMLProps<
  React.HTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> & {
  className?: string;
  titleChildren?: string;
  descriptionChildren?: React.ReactNode;
  errorList?: {
    children: ReactNode;
    reactListKey: string;
    href: string;
    "data-testid"?: string;
  }[];
};

export const ErrorSummary: React.FC<ErrorSummaryProps> = (props) => {
  const resolvedProps = {
    ...props,
    descriptionChildren: React.isValidElement(props.descriptionChildren)
      ? props.descriptionChildren
      : props.descriptionChildren?.toString(),
  };

  return <GDS.ErrorSummary {...resolvedProps} />;
};
