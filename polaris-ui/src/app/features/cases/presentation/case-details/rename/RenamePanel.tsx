import { useState, useMemo, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  Input,
  Spinner,
  ErrorSummary,
} from "../../../../../common/presentation/components";

import { RenameDocumentData } from "../../../domain/gateway/RenameDocumentData";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./RenamePanel.module.scss";

type NotesPanelProps = {
  documentName: string;
  documentId: string;
  renameDocuments: RenameDocumentData[];
  handleSaveRename: (documentId: string, newValue: string) => void;
  handleResetRenameData: (documentId: string) => void;
  handleClose: () => void;
};
const MAX_LENGTH = 252;

export const RenamePanel: React.FC<NotesPanelProps> = ({
  documentName,
  renameDocuments,
  documentId,
  handleClose,
  handleSaveRename,
  handleResetRenameData,
}) => {
  const cancelBtnRef = useRef(null);
  const errorSummaryRef = useRef(null);
  const trackEvent = useAppInsightsTrackEvent();
  const [newValue, setNewValue] = useState("");
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
    if (
      renameData?.saveRenameStatus === "failure" ||
      renameData?.saveRenameStatus === "initial"
    )
      setSavingState("initial");
    if (
      renameData?.saveRenameStatus === "saving" ||
      renameData?.saveRenameRefreshStatus === "updating"
    )
      setSavingState("saving");
    if (renameData?.saveRenameRefreshStatus === "updated")
      setSavingState("saved");
  }, [renameData]);

  const handleAddBtnClick = () => {
    if (!newValue.length) {
      setRenameErrorText("New name should not be empty");
      return;
    }
    if (newValue.length > MAX_LENGTH) {
      setRenameErrorText(
        `New name should be less than ${MAX_LENGTH} characters`
      );
      return;
    }
    if (newValue === documentName) {
      setRenameErrorText(`New name should be different from current name`);
      return;
    }
    if (cancelBtnRef.current) {
      (cancelBtnRef.current as HTMLElement).focus();
    }
    if (renameErrorText) {
      setRenameErrorText("");
    }
    saveRename();
  };

  const saveRename = () => {
    // trackEvent("Add Note", {
    //   documentId: documentId,
    //   documentCategory: documentCategory,
    // });

    handleSaveRename(documentId, newValue);
  };

  useEffect(() => {
    if (renameErrorText && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [renameErrorText]);

  const saveRenameSuccessLiveText = useMemo(() => {
    if (savingState === "saved") {
      return "Document renamed successfully";
    }
    return "";
  }, [savingState]);

  return (
    <div className={classes.renamePanel}>
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
              <Spinner diameterPx={5} ariaLabel={"spinner-animation"} />
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
          {saveRenameSuccessLiveText}
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
              data-testid={"notes-error-summary"}
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
            value={newValue}
            onChange={(value) => {
              setNewValue(value);
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
                  ref={cancelBtnRef}
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
