import React from "react";
import * as GDS from "govuk-react-jsx";

type CharacterCountProps = {
  id: string;
  label: {
    children: React.ReactNode;
  };
  hint?: {
    children: React.ReactNode;
  };
  errorMessage?: {
    children: React.ReactNode;
  };
  value: string;
  maxlength: number;
  name: string;
};
export const CharacterCount = React.forwardRef<
  HTMLSelectElement | null,
  CharacterCountProps
>((props, ref) => {
  return <GDS.CharacterCount {...props} ref={ref} />;
});
