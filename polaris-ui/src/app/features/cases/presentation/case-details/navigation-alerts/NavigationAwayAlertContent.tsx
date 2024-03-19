import React from "react";
import { Button } from "../../../../../common/presentation/components/Button";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { UnSavedRedactionDoc } from "../../../hooks/useNavigationAlert";
import classes from "./NavigationAwayAlertContent.module.scss";

type Props =
  | {
      type: "document";
      documentId: string;
      handleCancelAction: () => void;
      handleContinueAction: (documentIds: string[]) => void;
    }
  | {
      type: "casefile";
      handleCancelAction: () => void;
      handleContinueAction: (documentIds: string[]) => void;
      unSavedRedactionDocs: UnSavedRedactionDoc[];
      handleOpenPdf: (doc: {
        documentId: CaseDocumentViewModel["documentId"];
      }) => void;
    };

export const NavigationAwayAlertContent: React.FC<Props> = (props) => {
  const { type, handleCancelAction, handleContinueAction } = props;
  const headingText =
    type === "document"
      ? "You have unsaved redactions"
      : `You have ${
          props.unSavedRedactionDocs.length > 1
            ? `${props.unSavedRedactionDocs.length} documents`
            : `1 document`
        } with unsaved redactions`;

  const handleIgnoreBtnClick = () => {
    const documentIds =
      props.type === "document"
        ? [props.documentId]
        : props.unSavedRedactionDocs.map(({ documentId }) => documentId);
    handleContinueAction(documentIds);
  };
  return (
    <div className={classes.alertContent}>
      <h1 className="govuk-heading-l">{headingText}</h1>
      {type === "casefile" && (
        <div className={classes.documentLinks}>
          {props.unSavedRedactionDocs.map((caseDocument) => (
            <LinkButton
              key={caseDocument.documentId}
              onClick={() => props.handleOpenPdf(caseDocument)}
              dataTestId={`link-document-${caseDocument.documentId}`}
            >
              {caseDocument.presentationFileName}
            </LinkButton>
          ))}
        </div>
      )}
      <p>If you do not save the redactions the file will not be changed.</p>
      <div className={classes.actionButtonsWrapper}>
        <Button onClick={handleCancelAction} data-testid="btn-nav-return">
          Return to case file
        </Button>
        <LinkButton onClick={handleIgnoreBtnClick} dataTestId="btn-nav-ignore">
          Ignore
        </LinkButton>
      </div>
    </div>
  );
};
