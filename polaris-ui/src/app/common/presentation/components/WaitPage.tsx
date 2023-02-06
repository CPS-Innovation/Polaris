import { PageContentWrapper } from ".";

export const WaitPage: React.FC = (props) => (
  <PageContentWrapper>
    <div {...props}>
      <h1
        className="govuk-heading-xl"
        data-testid="txt-please-wait-page-heading"
      >
        Please wait...
      </h1>
    </div>
  </PageContentWrapper>
);
