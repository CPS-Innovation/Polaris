import * as GDS from "govuk-react-jsx";

export type RadiosProps = {
  fieldset?: {
    legend: {
      children: React.ReactNode;
      className?: string;
      isPageHeading?: boolean;
    };
  };
  hint?: {
    children: any;
  };
  className?: string;
  value: string | undefined;
  name: string;
  items: {
    reactListKey?: string;
    value: string | undefined;
    children: React.ReactNode;
    conditional?: any;
    disabled?: boolean // disabling only children not parent takes effect
  }[];
  onChange?: (value: string | undefined) => void;
};

export const Radios: React.FC<RadiosProps> = ({
  items,
  onChange: propOnChange,
  ...props
}) => {
  const processedItems = items.map((item) => ({
    ...item,
    reactListKey: item.reactListKey || item.value,
  }));

  const onChange: React.ChangeEventHandler<HTMLInputElement> = (event) => {
    if (propOnChange) {
      propOnChange(event.target.value);
    }
  };

  return <GDS.Radios items={processedItems} onChange={onChange} {...props} />;
};
