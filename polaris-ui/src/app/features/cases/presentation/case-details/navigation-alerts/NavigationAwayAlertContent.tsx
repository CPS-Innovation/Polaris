import React from "react";
import { Button } from "../../../../../common/presentation/components/Button";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import classes from "./NavigationAwayAlertContent.module.scss";
type Props = {
  type?: "document" | "casefile";
  activeRedactionDocs?: {
    documentId: number;
    tabSafeId: string;
    presentationFileName: string;
  }[];
  handleCancelAction: () => void;
  handleContinueAction: () => void;
  handleOpenPdf?: (doc: { tabSafeId: string; documentId: number }) => void;
};

export const NavigationAwayAlertContent: React.FC<Props> = ({
  activeRedactionDocs = [],
  type = "document",
  handleCancelAction,
  handleContinueAction,
  handleOpenPdf,
}) => {
  const headingText =
    type === "document"
      ? "You have unsaved redactions"
      : `You have ${
          activeRedactionDocs.length > 1
            ? `${activeRedactionDocs.length} documents`
            : `1 document`
        } with unsaved redactions`;
  return (
    <div className={classes.alertContent}>
      <h1 className="govuk-heading-l">{headingText}</h1>
      {type === "casefile" && (
        <div className={classes.documentLinks}>
          {activeRedactionDocs.map((caseDocument) => (
            <a
              href={`#${caseDocument.tabSafeId}`}
              onClick={(ev) => {
                handleOpenPdf && handleOpenPdf(caseDocument);
              }}
              data-testid={`link-document-${caseDocument.documentId}`}
            >
              {caseDocument.presentationFileName}
            </a>
          ))}
        </div>
      )}
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
