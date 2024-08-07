import { useState, useMemo, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  Input,
  Spinner,
  ErrorSummary,
} from "../../../../../common/presentation/components";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import { RenameDocumentData } from "../../../domain/gateway/RenameDocumentData";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { Classification } from "../../../domain/gateway/PipelineDocument";
import classes from "./RenamePanel.module.scss";

type RenamePanelProps = {
  documentName: string;
  documentId: string;
  documentType: string;
  classification: Classification;
  renameDocuments: RenameDocumentData[];
  handleSaveRename: (documentId: string, newName: string) => void;
  handleResetRenameData: (documentId: string) => void;
  handleClose: () => void;
};
const MAX_LENGTH = 252;

export const RenamePanel: React.FC<RenamePanelProps> = ({
  documentName,
  renameDocuments,
  documentId,
  documentType,
  classification,
  handleClose,
  handleSaveRename,
  handleResetRenameData,
}) => {
  useLastFocus(`#document-housekeeping-actions-dropdown-${documentId}`);
  const closeBtnRef = useRef(null);
  const errorSummaryRef = useRef(null);
  const trackEvent = useAppInsightsTrackEvent();
  const [newName, setNewName] = useState(documentName);
  const [renameErrorText, setRenameErrorText] = useState("");

  const [savingState, setSavingState] = useState<
    "initial" | "saving" | "saved" | "failed"
  >("initial");

  const renameData = useMemo(() => {
    return renameDocuments.find((data) => data.documentId === documentId);
  }, [renameDocuments, documentId]);

  useEffect(() => {
    handleResetRenameData(documentId);
  }, []);

  useEffect(() => {
    if (closeBtnRef.current) {
      (closeBtnRef.current as HTMLElement).focus();
    }
  }, [savingState]);

  useEffect(() => {
    if (
      renameData?.saveRenameStatus === "failure" ||
      renameData?.saveRenameStatus === "initial"
    ) {
      setSavingState("initial");
      return;
    }
    if (
      renameData?.saveRenameStatus === "saving" ||
      renameData?.saveRenameRefreshStatus === "updating"
    ) {
      setSavingState("saving");
      return;
    }
    if (renameData?.saveRenameRefreshStatus === "updated") {
      setSavingState("saved");
    }
  }, [renameData]);

  const handleAddBtnClick = () => {
    if (!newName.length) {
      setRenameErrorText("Enter a new name");
      return;
    }
    if (newName.length > MAX_LENGTH) {
      setRenameErrorText(`New name must be ${MAX_LENGTH} characters or less`);
      return;
    }
    if (newName === documentName) {
      setRenameErrorText(`New name should be different from current name`);
      return;
    }
    if (renameErrorText) {
      setRenameErrorText("");
    }
    saveRename();
  };

  const saveRename = () => {
    trackEvent("Save Rename Document", {
      documentId: documentId,
      documentType: documentType,
      classification: classification,
      oldDocumentName: documentName,
      newDocumentName: newName,
    });

    handleSaveRename(documentId, newName);
  };

  useEffect(() => {
    if (renameErrorText && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [renameErrorText]);

  const saveRenameLiveText = useMemo(() => {
    if (savingState === "saved") {
      return "Document renamed successfully saved to CMS";
    }
    if (savingState === "saving") {
      return "Saving renamed document to CMS";
    }
    return "";
  }, [savingState]);

  return (
    <div className={classes.renamePanel} data-testid="rename-panel">
      <div className={classes.renameHeader}>
        {" "}
        {savingState === "initial" && (
          <h3 className={classes.renameTitle}>
            {" "}
            Rename -{" "}
            <span className={classes.renameDocumentName}>{documentName}</span>
          </h3>
        )}
        {savingState === "saving" && (
          <div
            className={classes.savingBanner}
            data-testid="rl-saving-redactions"
          >
            <div className={classes.spinnerWrapper}>
              <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
            </div>
            <h3 className={classes.bannerText}>
              Saving renamed document to CMS
            </h3>
          </div>
        )}
        {savingState === "saved" && (
          <div
            className={classes.savedBanner}
            data-testid="rl-saved-redactions"
          >
            <WhiteTickIcon className={classes.whiteTickIcon} />
            <h3 className={classes.bannerText}>
              Document renamed successfully saved to CMS
            </h3>
          </div>
        )}
        <LinkButton
          dataTestId="btn-close-rename-panel"
          type="button"
          className={classes.renamePanelCloseBtn}
          ariaLabel="close rename panel"
          disabled={savingState === "saving"}
          onClick={handleClose}
        >
          <CloseIcon height={"2.5rem"} width={"2.5rem"} />
        </LinkButton>
        <div
          role="status"
          aria-live="polite"
          className={classes.visuallyHidden}
        >
          {saveRenameLiveText}
        </div>
      </div>
      <div className={classes.renameBody}>
        {renameErrorText && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={classes.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"rename-error-summary"}
              className={classes.errorSummary}
              errorList={[
                {
                  reactListKey: "1",
                  children: renameErrorText,
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
              renameErrorText
                ? {
                    children: renameErrorText,
                  }
                : undefined
            }
            id="rename-text-input"
            data-testid={"rename-text-input"}
            value={newName}
            onChange={(value) => {
              setNewName(value);
            }}
            label={{
              children: "What is the new name of your document?",
              className: "govuk-label--s",
              htmlFor: "rename-text-input",
            }}
            disabled={savingState !== "initial"}
          />

          <div className={classes.btnWrapper}>
            {savingState === "saved" && (
              <Button
                ref={closeBtnRef}
                type="button"
                className={classes.closeBtn}
                data-testid="btn-close-rename"
                onClick={handleClose}
              >
                close
              </Button>
            )}
            {savingState !== "saved" && (
              <>
                <Button
                  disabled={savingState === "saving"}
                  type="submit"
                  className={classes.saveBtn}
                  data-testid="btn-save-rename"
                  onClick={() => handleAddBtnClick()}
                >
                  Accept and save
                </Button>

                <LinkButton
                  className={classes.cancelBtn}
                  onClick={() => handleClose()}
                  dataTestId="btn-cancel-rename"
                  ariaLabel="cancel rename"
                  id="btn-cancel-rename"
                  disabled={savingState === "saving"}
                >
                  Cancel
                </LinkButton>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
