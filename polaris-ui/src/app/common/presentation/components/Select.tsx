import * as GDS from "govuk-react-jsx";
import React from "react";
import { LabelProps } from "./Label";

type SelectProps = React.DetailedHTMLProps<
  React.SelectHTMLAttributes<HTMLSelectElement>,
  HTMLSelectElement
> & {
  errorMessage?: {
    className?: string;
    children: React.ReactNode;
    visuallyHiddenText?: string;
    attributes?: React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLSpanElement>,
      HTMLSpanElement
    >;
  };
  formGroup?: { className: string };
  hint?: string;
  label?: LabelProps;
  id?: string;
  items: {
    reactListKey?: string;
    value: string;
    children: React.ReactNode;
    optionAttributes?: React.DetailedHTMLProps<
      React.OptionHTMLAttributes<HTMLOptionElement>,
      HTMLOptionElement
    >;
  }[];
};

export const Select = React.forwardRef<HTMLSelectElement | null, SelectProps>(
  (props, ref) => {
    return <GDS.Select {...props} ref={ref} />;
  }
);
