import * as GDS from "govuk-react-jsx";
import { LabelProps } from "./Label";

type InputProps = Omit<
  React.DetailedHTMLProps<
    React.InputHTMLAttributes<HTMLInputElement>,
    HTMLInputElement
  >,
  "onChange"
> & {
  className?: string;
  describedBy?: string;
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
  name?: string;
  id?: string;
  prefix?: React.ReactNode;
  suffix?: React.ReactNode;
  // as a convenience, let consumer just deal with the event value rather than the event
  onChange?: (val: string) => void;
};

export const Input: React.FC<InputProps> = ({ onChange, ...props }) => {
  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) =>
    onChange && onChange(event.target.value);

  return <GDS.Input {...props} onChange={handleChange}></GDS.Input>;
};
