import { KeyboardEvent } from "react";
import { Input } from "../../../../../common/presentation/components";
import classes from "./SearchBox.module.scss";

type Props = {
  labelText: string;
  value: undefined | string;
  id: string;
  handleChange: (val: string) => void;
  handleSubmit: () => void;
  "data-testid"?: string;
};

export const SearchBox: React.FC<Props> = ({
  value,
  handleChange,
  handleSubmit,
  labelText,
  id,
  "data-testid": dataTestId,
}) => {
  const handleKeyPress = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleSubmit();
    }
  };

  return (
    <div className={classes.container}>
      <Input
        id={id}
        data-testid={dataTestId && `input-${dataTestId}`}
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyPress}
        label={{
          children: labelText,
          className: "govuk-label--s",
          htmlFor: id,
        }}
        suffix={{
          children: (
            <button
              data-testid={dataTestId && `btn-${dataTestId}`}
              className={classes.button}
              type="submit"
              onClick={handleSubmit}
            ></button>
          ),
          className: classes.suffix,
        }}
      />
    </div>
  );
};
