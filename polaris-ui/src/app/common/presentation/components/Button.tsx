import * as GDS from "govuk-react-jsx";
import React, { forwardRef } from "react";
type ButtonProps = React.DetailedHTMLProps<
  React.ButtonHTMLAttributes<HTMLButtonElement>,
  HTMLButtonElement
> & {
  element?: "a" | "button" | "input";
  href?: string;
  to?: string;
  isStartButton?: boolean;
  disabled?: boolean;
  className?: string;
  preventDoubleClick?: boolean;
  name?: string;
  type?: string;
};

export const Button = forwardRef<HTMLButtonElement | null, ButtonProps>(
  (props, ref) => {
    return <GDS.Button {...props} ref={ref} />;
  }
);
