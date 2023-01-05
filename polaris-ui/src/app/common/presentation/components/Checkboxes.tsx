import * as GDS from "govuk-react-jsx";
import React from "react";

type CheckboxesProps = Omit<
  React.DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement>,
  "onChange"
> & {
  className?: string;
  errorMessage?: {
    className?: string;
    children: React.ReactNode;
    visuallyHiddenText?: string;
    attributes?: React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLSpanElement>,
      HTMLSpanElement
    >;
  };
  fieldSet?: React.DetailedHTMLProps<
    React.HtmlHTMLAttributes<HTMLFieldSetElement>,
    HTMLFieldSetElement
  >;
  formGroup?: { className: string };
  hint?: string;
  idPrefix?: string;
  items: Omit<
    React.DetailedHTMLProps<
      React.InputHTMLAttributes<HTMLInputElement>,
      HTMLInputElement
    >,
    "onChange" | "onBlur"
  > &
    { className?: string }[];

  name: string;
  onChange?: (ev: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (ev: React.FocusEvent<HTMLInputElement>) => void;
  describedByProp?: string;
};

export const Checkboxes: React.FC<CheckboxesProps> = (props) => {
  return <GDS.Checkboxes {...props} />;
};
