import { ReactChildren } from "react";
import { PageContentWrapper } from "../PageContentWrapper";

type Props = { error: Error | undefined; children?: ReactChildren };

export const ErrorPage: React.FC<Props> = ({ error, children }: Props) => {
  return (
    <PageContentWrapper>
      <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
        Sorry, there is a problem with the service
      </h1>

      <>
        <p className="govuk-body-l">
          Please try this case again later, or{" "}
          <a href={process.env.PUBLIC_URL} className="govuk-link">
            click here to start a new search
          </a>
          .
        </p>
      </>

      <p className="govuk-inset-text">{error?.toString()}</p>
    </PageContentWrapper>
  );
};
