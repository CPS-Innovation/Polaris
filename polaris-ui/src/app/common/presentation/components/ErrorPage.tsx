import { ApiError } from "../../errors/ApiError";
import { CmsAuthError } from "../../errors/CmsAuthError";
import { PageContentWrapper } from "./PageContentWrapper";

type Props = { error: Error | undefined };

export const ErrorPage = ({ error }: Props) => (
  <PageContentWrapper>
    <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
      Sorry, there is a problem with the service
    </h1>

    <p className="govuk-body-l">
      {error instanceof ApiError || error instanceof CmsAuthError
        ? error.customMessage
        : error?.toString()}
    </p>

    <p className="govuk-body">
      Try again later, or{" "}
      <a href={process.env.PUBLIC_URL} className="govuk-link">
        click here to start a new search
      </a>
      .
    </p>
    <p className="govuk-inset-text">{error?.name}</p>
  </PageContentWrapper>
);
