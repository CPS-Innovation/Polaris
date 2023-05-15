import { PageContentWrapper } from "./PageContentWrapper";

type Props = { error: Error | undefined };

export const ErrorPage = ({ error }: Props) => (
  <PageContentWrapper>
    <h1 className="govuk-heading-xl" data-testid="txt-error-page-heading">
      Sorry, there is a problem with the service
    </h1>
    <p>
      Try again later, or{" "}
      <a href={process.env.PUBLIC_URL} className="govuk-link">
        click here to start a new search
      </a>
      .
    </p>
    <div className="govuk-inset-text">{error?.toString()}</div>
  </PageContentWrapper>
);
