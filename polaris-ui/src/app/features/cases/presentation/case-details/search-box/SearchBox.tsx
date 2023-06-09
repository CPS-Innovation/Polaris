import { KeyboardEvent } from "react";
import { Input } from "../../../../../common/presentation/components";
import { ReactComponent as SearchIcon } from "../../../../../common/presentation/svgs/searchIcon.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./SearchBox.module.scss";

type Props = {
  labelText: string;
  value: undefined | string;
  id: string;
  handleChange: (val: string) => void;
  handleSubmit: () => void;
  trackEventKey:
    | "Search Case Documents From Case Details"
    | "Search Case Documents From Document Search";
  "data-testid"?: string;
};

export const SearchBox: React.FC<Props> = ({
  value,
  handleChange,
  handleSubmit,
  labelText,
  id,
  "data-testid": dataTestId,
  trackEventKey,
}) => {
  const trackEvent = useAppInsightsTrackEvent();

  const handleSearch = () => {
    trackEvent(trackEventKey, { searchParameter: value });
    handleSubmit();
  };
  const handleKeyPress = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleSearch();
      event.preventDefault();
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
              aria-label="Search"
              data-testid={dataTestId && `btn-${dataTestId}`}
              className={classes.button}
              type="submit"
              onClick={handleSearch}
            >
              <SearchIcon width={"20px"} height={"20px"} />
            </button>
          ),
          className: classes.suffix,
        }}
      />
    </div>
  );
};
