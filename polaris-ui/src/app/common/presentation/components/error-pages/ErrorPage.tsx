import { CmsAuthError } from "../../../errors/CmsAuthError";
import { PageContentWrapper } from "../PageContentWrapper";

type Props = { error: (Error & { code?: number }) | undefined };

export const ErrorPage = ({ error }: Props) => {
  const isAnAuthError = error instanceof CmsAuthError;
  return (
    <PageContentWrapper>
      {error?.code === 403 ? (
        <>
          <h1 className="govuk-heading-1" data-testid="txt-error-page-heading">
            You do not have access to this case
          </h1>
          <p className="govuk-body-l">
            This case is assigned to a unit you do not have access to.
          </p>
          <a href={process.env.PUBLIC_URL} className="govuk-link">
            Search for another case
          </a>
        </>
      ) : (
        <>
          <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
            Sorry, there is a problem with the service
          </h1>

          <p className="govuk-body-l">
            Please try this case again later, or{" "}
            <a href={process.env.PUBLIC_URL} className="govuk-link">
              click here to start a new search
            </a>
            .
          </p>

          <p className="govuk-inset-text">{error?.toString()}</p>
        </>
      )}
    </PageContentWrapper>
  );
};
