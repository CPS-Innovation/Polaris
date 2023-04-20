import React from "react";
import { useQueryParamsState } from "../../../../common/hooks/useQueryParamsState";
import { CaseSearchQueryParams } from "../../types/CaseSearchQueryParams";
import {
  Button,
  ErrorSummary,
  Hint,
  Input,
} from "../../../../common/presentation/components";
import { useSearchInputLogic } from "../../hooks/useSearchInputLogic";
import classes from "./index.module.scss";
import { PageContentWrapper } from "../../../../common/presentation/components";
import { useAppInsightsTrackEvent } from "../../../../common/hooks/useAppInsightTrackEvent";
export const path = "/case-search";

const validationFailMessage = "Enter a URN in the right format";

const Page: React.FC = () => {
  const trackEvent = useAppInsightsTrackEvent();
  const { urn: urnFromSearchParams, setParams } =
    useQueryParamsState<CaseSearchQueryParams>();

  const { handleChange, handleKeyPress, handleSubmit, isError, urn } =
    useSearchInputLogic({ urnFromSearchParams, setParams });

  const handleSearch = () => {
    trackEvent("Search URN");
    handleSubmit();
  };

  return (
    <PageContentWrapper>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          {isError && (
            <ErrorSummary
              errorList={[
                {
                  reactListKey: "1",
                  children: validationFailMessage,
                  href: "#urn",
                  "data-testid": "link-validation-urn",
                },
              ]}
            />
          )}

          <h1 className="govuk-heading-xl">
            Find a case
            <Hint className={classes.hint}>
              Search and review a CPS case in England and Wales
            </Hint>
          </h1>

          <div className="govuk-form-group">
            <Input
              id="urn"
              name="urn"
              onChange={handleChange}
              onKeyPress={handleKeyPress}
              value={urn}
              data-testid="input-search-urn"
              errorMessage={
                isError
                  ? {
                      children: (
                        <span data-testid="input-search-urn-error">
                          {validationFailMessage}
                        </span>
                      ),
                    }
                  : undefined
              }
              label={{
                className: "govuk-label--s",
                children: "Search for a case URN",
              }}
            />
          </div>
          <Button onClick={handleSearch} data-testid="button-search">
            Search
          </Button>
        </div>
      </div>
    </PageContentWrapper>
  );
};

export default Page;
