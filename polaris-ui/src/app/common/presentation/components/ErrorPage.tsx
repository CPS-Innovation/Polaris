import { CmsAuthError } from "../../errors/CmsAuthError";
import { PageContentWrapper } from "./PageContentWrapper";

type Props = { error: Error | undefined };

export const ErrorPage = ({ error }: Props) => {
  const isAnAuthError = error instanceof CmsAuthError;
  return (
    <PageContentWrapper>
      <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
        Sorry, there is a problem with the service
      </h1>
      {isAnAuthError && (
        <>
          <p className="govuk-body-l">
            {error.customMessage || error?.toString()}
          </p>
          <p className="govuk-body-l">
            Casework App only works if you are using it in the same browser
            window that you are using CMS. Usually this means that you would
            have CMS open in one browser tab and Casework App open in another
            tab.
          </p>
          <p className="govuk-body-l">
            If you have CMS open in another browser tab you could try logging in
            again to CMS in that tab. Otherwise, try opening and logging in to
            CMS in a new browser tab.
          </p>
          <p className="govuk-body-l">
            Once you have logged in to CMS try returning to this tab and{" "}
            <a href={window.location.href}>
              clicking here to reload this screen
            </a>
            .
          </p>
        </>
      )}
      {!isAnAuthError && (
        <>
          <p className="govuk-body-l">
            Please try this case again later, or{" "}
            <a href={process.env.PUBLIC_URL} className="govuk-link">
              click here to start a new search
            </a>
            .
          </p>
        </>
      )}

      <p className="govuk-inset-text">{error?.toString()}</p>
    </PageContentWrapper>
  );
};
