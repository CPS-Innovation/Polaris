import React from "react";
import { Button } from "../../../../../common/presentation/components/Button";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import classes from "./DocumentNavigationAlertContent.module.scss";
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

export const DocumentNavigationAlertContent: React.FC<Props> = ({
  activeRedactionDocs = [],
  type = "document",
  handleCancelAction,
  handleContinueAction,
  handleOpenPdf,
}) => {
  return (
    <div className={classes.documentAlertContent}>
      {type === "document" && (
        <h1 className="govuk-heading-l">You have unsaved redactions</h1>
      )}
      {type === "casefile" && (
        <h1 className="govuk-heading-l">You have unsaved redactions</h1>
      )}

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
