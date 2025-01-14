import React from "react";
import * as GDS from "govuk-react-jsx";

type DateInputProps = {
  id: string;
  hint?: {
    children: React.ReactNode;
  };
  items: {
    id: string;
    className: string;
    name: string;
    value?: string;
    disabled?: boolean;
  }[];
  errorMessage?: {
    children: React.ReactNode;
  };
  fieldset?: {
    legend: {
      children: React.ReactNode;
    };
  };
  namePrefix: string;
  onChange: (value: React.ChangeEvent<HTMLInputElement>) => void;
  disabled?: boolean;
};
export const DateInput: React.FC<DateInputProps> = (props) => {
  return <GDS.DateInput {...props} />;
};
