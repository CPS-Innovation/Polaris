import { useState, useMemo, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  Input,
  ErrorSummary,
} from "../../../../../common/presentation/components";

import { RenameDocumentData } from "../../../domain/gateway/RenameDocumentData";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./RenamePanel.module.scss";

type NotesPanelProps = {
  documentName: string;
  documentId: string;
  renameDocuments: RenameDocumentData[];
  handleSaveRename: (documentId: string, newValue: string) => void;
  handleClose: () => void;
};

export const RenamePanel: React.FC<NotesPanelProps> = ({
  documentName,
  renameDocuments,
  documentId,
  handleClose,
  handleSaveRename,
}) => {
  const cancelBtnRef = useRef(null);
  const errorSummaryRef = useRef(null);
  const trackEvent = useAppInsightsTrackEvent();
  const [newValue, setNewValue] = useState("");
  const [oldValue, setOldValue] = useState("");
  const [renameError, setRenameError] = useState(false);

  const handleAddBtnClick = () => {
    if (!newValue.length) {
      setRenameError(true);
      if (renameError && errorSummaryRef.current) {
        (errorSummaryRef?.current as HTMLButtonElement).focus();
      }
      return;
    }

    if (cancelBtnRef.current) {
      (cancelBtnRef.current as HTMLElement).focus();
    }
    if (renameError) {
      setRenameError(false);
    }
    saveRename();
  };

  const saveRename = () => {
    // trackEvent("Add Note", {
    //   documentId: documentId,
    //   documentCategory: documentCategory,
    // });
    setOldValue(newValue);
    setNewValue("");
    handleSaveRename(documentId, newValue);
  };

  const renameData = useMemo(() => {
    return renameDocuments.find((data) => data.documentId === documentId);
  }, [renameDocuments, documentId]);

  useEffect(() => {
    if (renameData?.saveRenameStatus === "failure") {
      setNewValue(oldValue);
    }
  }, [renameData?.saveRenameStatus]);

  useEffect(() => {
    if (renameError && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [renameError]);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (renameData?.saveRenameStatus === "success") {
        handleClose();
      }
    }, 500);
    return () => {
      clearTimeout(timer);
    };
  }, [renameData?.saveRenameStatus, handleClose]);

  const saveRenameSuccessLiveText = useMemo(() => {
    if (renameData?.saveRenameStatus === "success") {
      return "Document renamed successfully";
    }
    return "";
  }, [renameData?.saveRenameStatus]);

  return (
    <div className={classes.renamePanel}>
      <div className={classes.renameHeader}>
        {" "}
        <h3 className={classes.renameTitle}>
          {" "}
          Rename -{" "}
          <span className={classes.renameDocumentName}>{documentName}</span>
        </h3>
        <LinkButton
          dataTestId="btn-close-rename-panel"
          type="button"
          className={classes.renamePanelCloseBtn}
          ariaLabel="close rename panel"
          onClick={() => handleClose()}
        >
          <CloseIcon height={"2.5rem"} width={"2.5rem"} />
        </LinkButton>
        <div
          role="status"
          aria-live="polite"
          className={classes.visuallyHidden}
        >
          {saveRenameSuccessLiveText}
        </div>
      </div>
      <div className={classes.renameBody}>
        {renameError && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={classes.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"notes-error-summary"}
              className={classes.errorSummary}
              errorList={[
                {
                  reactListKey: "1",
                  children: `New name should not be empty`,
                  href: "#rename-text-input",
                  "data-testid": "rename-text-input-link",
                },
              ]}
            />
          </div>
        )}
        <div>
          <Input
            errorMessage={
              renameError
                ? {
                    children: `New name should not be empty`,
                  }
                : undefined
            }
            id="rename-text-input"
            data-testid={"rename-text-input"}
            value={newValue}
            onChange={(value) => {
              setNewValue(value);
            }}
            label={{
              children: "What is the new name of your document?",
              className: "govuk-label--s",
              htmlFor: "rename-text-input",
            }}
          />

          <div className={classes.btnWrapper}>
            <Button
              disabled={!newValue.length}
              type="submit"
              className={classes.saveBtn}
              data-testid="btn-add-note"
              onClick={handleAddBtnClick}
            >
              Save and close
            </Button>

            <LinkButton
              ref={cancelBtnRef}
              className={classes.cancelBtn}
              onClick={() => handleClose()}
              dataTestId="btn-cancel-notes"
              ariaLabel="cancel notes"
              id="btn-cancel-notes"
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      </div>
    </div>
  );
};
