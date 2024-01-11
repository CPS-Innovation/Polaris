import React from "react";
import * as GDS from "govuk-react-jsx";

type TextAreaProps = {
  name: string;
  id: string;
  label: {
    children: React.ReactNode;
  };
  value: string;
  hint?: {
    children: React.ReactNode;
  };
  maxLength?: string;
  onChange: (value: React.ChangeEvent<HTMLTextAreaElement>) => void;
};
export const TextArea = React.forwardRef<
  HTMLSelectElement | null,
  TextAreaProps
>((props, ref) => {
  return <GDS.Textarea {...props} ref={ref} />;
});
