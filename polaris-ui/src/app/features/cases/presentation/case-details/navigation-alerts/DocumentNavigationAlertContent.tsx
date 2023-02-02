import React from "react";
import { Button } from "../../../../../common/presentation/components/Button";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import classes from "./DocumentNavigationAlertContent.module.scss";
type Props = {
  handleCancelAction: () => void;
  handleContinueAction: () => void;
};

export const DocumentNavigationAlertContent: React.FC<Props> = ({
  handleCancelAction,
  handleContinueAction,
}) => {
  return (
    <div className={classes.documentAlertContent}>
      <h1 className="govuk-heading-l">You have unsaved redactions</h1>
      <p>If you do not save the redactions the file will not be changed.</p>
      <div className={classes.actionButtonsWrapper}>
        <Button onClick={handleCancelAction} data-testid="btn-nav-return">
          Return to case file
        </Button>
        <LinkButton onClick={handleContinueAction} data-testid="btn-nav-ignore">
          Ignore
        </LinkButton>
      </div>
    </div>
  );
};
