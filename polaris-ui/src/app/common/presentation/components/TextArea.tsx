import React from "react";
import * as GDS from "govuk-react-jsx";

type TextAreaProps = {
  name: string;
  id: string;
  label: {
    children: React.ReactNode;
  };
  value: string;
  onChange: (value: React.ChangeEvent<HTMLTextAreaElement>) => void;
};
export const TextArea: React.FC<TextAreaProps> = (props) => {
  return <GDS.Textarea {...props} />;
};
