import { ReactNode } from "react";
import { CmsAuthError } from "../../../errors/CmsAuthError";
import { PageContentWrapper } from "../PageContentWrapper";

type Props = { error: CmsAuthError; children?: ReactNode };

export const AuthErrorPage: React.FC<Props> = ({ error, children }: Props) => {
  return (
    <PageContentWrapper>
      <h1 className="govuk-heading-l" data-testid="txt-error-page-heading">
        Sorry, it has not been possible to connect to your CMS session.
      </h1>

      <p className="govuk-body-l">Don't worry, this can be resolved.</p>

      <h2 className="govuk-heading-m">What may have happened?</h2>
      <p className="govuk-body-l">{error.customMessage || error?.toString()}</p>

      <h2 className="govuk-heading-m">Why is your CMS session relevant?</h2>
      <p className="govuk-body-l">
        Casework App only works if you are using it in the same browser that you
        are using CMS. Usually this means that you would have CMS open in your
        Edge browser in one tab and Casework App open in another tab.
      </p>

      <h2 className="govuk-heading-m">What should you do now?</h2>
      <p className="govuk-body-l">
        If you have CMS open in another browser tab you should go to that tab
        and log in to CMS again. Otherwise you should open CMS in a new tab in
        this browser and log in there.
      </p>
      <p className="govuk-body-l">
        Once you have logged in to CMS return to this tab and{" "}
        {children || (
          <a href={window.location.href}>click here to reload this screen</a>
        )}
        .
      </p>

      <h2 className="govuk-heading-s">Technical information</h2>
      <p className="govuk-inset-text">{error?.toString()}</p>
    </PageContentWrapper>
  );
};
