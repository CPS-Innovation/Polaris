import * as GDS from "govuk-react-jsx";
import React from "react";

type CheckBoxesItems = Omit<
  React.DetailedHTMLProps<
    React.InputHTMLAttributes<HTMLInputElement>,
    HTMLInputElement
  >,
  "onChange" | "onBlur"
> & { className?: string } & {
  conditional?: { children: React.ReactNode[] };
};
export type CheckboxesProps = Omit<
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
  fieldset?: { legend: { children: string } };
  formGroup?: { className: string };
  hint?: string;
  idPrefix?: string;
  items: CheckBoxesItems[];

  name: string;
  onChange?: (ev: React.ChangeEvent<HTMLInputElement>) => void;
  onBlur?: (ev: React.FocusEvent<HTMLInputElement>) => void;
  describedByProp?: string;
};

export const Checkboxes: React.FC<CheckboxesProps> = (props) => {
  return <GDS.Checkboxes {...props} />;
};
