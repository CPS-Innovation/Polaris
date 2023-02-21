import {
  BackLink,
  Button,
  ErrorSummary,
  Hint,
  Input,
} from "../../../../common/presentation/components";
import { CaseSearchQueryParams } from "../../types/CaseSearchQueryParams";
import { useQueryParamsState } from "../../../../common/hooks/useQueryParamsState";

import { useSearchInputLogic } from "../../hooks/useSearchInputLogic";
import { generatePath, Link } from "react-router-dom";
import { path as casePath } from "../case-details";
import {
  formatDate,
  CommonDateTimeFormats,
} from "../../../../common/utils/dates";
import { BackLinkingPageProps } from "../../../../common/presentation/types/BackLinkingPageProps";
import { PageContentWrapper } from "../../../../common/presentation/components";
import { WaitPage } from "../../../../common/presentation/components";
import { useApi } from "../../../../common/hooks/useApi";
import { searchUrn } from "../../api/gateway-api";

import classes from "./index.module.scss";
import { SectionBreak } from "../../../../common/presentation/components";

export const path = "/case-search-results";

const validationFailMessage = "Enter a URN in the right format";

type Props = BackLinkingPageProps & {};

const Page: React.FC<Props> = ({ backLinkProps }) => {
  const {
    urn: initialUrn,
    setParams,
    search,
  } = useQueryParamsState<CaseSearchQueryParams>();

  const { handleChange, handleKeyPress, handleSubmit, isError, urn } =
    useSearchInputLogic({ initialUrn, setParams });

  const state = useApi(searchUrn, [initialUrn!])!;

  if (state.status === "loading" || state.status === "initial") {
    return <WaitPage />;
  }

  if (state.status === "failed") {
    throw state.error;
  }

  const { data } = state;

  return (
    <>
      <BackLink to={backLinkProps.to}>{backLinkProps.label}</BackLink>
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
            <h1 className="govuk-heading-xl">Find a case</h1>

            <div className={classes.search}>
              <Input
                id="urn"
                name="urn"
                onChange={handleChange}
                onKeyPress={handleKeyPress}
                value={urn}
                autoFocus
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
                formGroup={{
                  className: "govuk-!-width-full",
                }}
              />
              <Button onClick={handleSubmit} data-testid="button-search">
                Search
              </Button>
            </div>

            <div className={classes.results}>
              <p>
                We've found <b data-testid="txt-result-count">{data.length}</b>
                {data.length !== 1
                  ? " cases that match "
                  : " case that matches "}
                <span data-testid="txt-result-urn">{initialUrn}</span>
              </p>

              <SectionBreak />

              {data.map((item, index) => (
                <div key={item.id} className={classes.result}>
                  <h2 className="govuk-heading-m ">
                    <Link
                      to={{
                        pathname: generatePath(casePath, {
                          urn: encodeURIComponent(item.uniqueReferenceNumber),
                          id: item.id,
                        }),
                        state: search,
                      }}
                      data-testid={`link-${item.uniqueReferenceNumber}`}
                      className="govuk-link"
                    >
                      {item.uniqueReferenceNumber}
                    </Link>
                    <Hint className={classes.defendantName}>
                      {`${item.leadDefendantDetails.surname}, 
                        ${item.leadDefendantDetails.firstNames}
                        ${item.numberOfDefendants > 1 ? "and others" : ""}`}
                      <br />
                      Date of birth:{" "}
                      {formatDate(
                        item.leadDefendantDetails.dob,
                        CommonDateTimeFormats.ShortDateFullTextMonth
                      )}
                    </Hint>
                  </h2>
                  <div>
                    <div className={classes["result-offence"]}>
                      <div className={classes["result-offence-line"]}>
                        <span>Status:</span>
                        <span>
                          {item.isCaseCharged ? "Charged" : "Not yet charged"}
                        </span>
                      </div>
                      {item.isCaseCharged &&
                      item.headlineCharge.nextHearingDate ? (
                        <div className={classes["result-offence-line"]}>
                          <span>Court hearing:</span>
                          <span>
                            {formatDate(
                              item.headlineCharge.nextHearingDate,
                              CommonDateTimeFormats.ShortDateFullTextMonth
                            )}
                          </span>
                        </div>
                      ) : null}
                      {item.headlineCharge.date ? (
                        <div className={classes["result-offence-line"]}>
                          <span>Date of offence:</span>
                          <span>
                            {formatDate(
                              item.headlineCharge.date,
                              CommonDateTimeFormats.ShortDateFullTextMonth
                            )}
                          </span>
                        </div>
                      ) : null}
                      <div className={classes["result-offence-line"]}>
                        <span>
                          {item.isCaseCharged ? "Charges:" : "Proposed:"}
                        </span>
                        <span>{item.headlineCharge.charge}</span>
                      </div>
                    </div>
                  </div>
                  {index < data.length - 1 && <SectionBreak />}
                </div>
              ))}
            </div>
          </div>
        </div>
      </PageContentWrapper>
    </>
  );
};

export default Page;
