import { useState, useMemo } from "react";
import {
  Button,
  LinkButton,
  Modal,
  Select,
} from "../../../../../common/presentation/components";
import { ReactComponent as DeleteIcon } from "../../../../../common/presentation/svgs/deleteIcon.svg";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { IPageDeleteRedaction } from "../../../domain/IPageDeleteRedaction";
import classes from "./DocumentButton.module.scss";
type DocumentButtonsProps = {
  documentId: string;
  pageNumber: number;
  redactionTypesData: RedactionTypeData[];
  pageDeleteRedactions: IPageDeleteRedaction[];
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: (id: string) => void;
};

export const DocumentButtons: React.FC<DocumentButtonsProps> = ({
  documentId,
  pageNumber,
  redactionTypesData,
  pageDeleteRedactions,
  handleAddRedaction,
  handleRemoveRedaction,
}) => {
  const getMappedRedactionTypes = () => {
    const defaultOption = {
      value: "",
      children: "-- Please select --",
      disabled: true,
    };
    const deleteRedactionTypeIds = ["16", "17", "18"];
    const mappedRedactionType = redactionTypesData
      .filter((item) => deleteRedactionTypeIds.includes(item.id))
      .map((item) => ({
        value: item.id,
        children: item.name,
      }));

    return [defaultOption, ...mappedRedactionType];
  };
  const isPageDeleted = useMemo(() => {
    return pageDeleteRedactions.find(
      (redaction) => redaction.pageNumber === pageNumber
    );
  }, [pageDeleteRedactions, pageNumber]);
  const [showModal, setShowModal] = useState(false);
  const [deleteRedactionType, setDeleteRedactionType] = useState("");
  const handleRotate = (e: any) => {
    console.log("handleRotate>>", pageNumber);
  };
  const handleDelete = () => {
    console.log("handleDelete>>", pageNumber);
    setShowModal(true);
  };

  const handleConfirmBtnClick = () => {
    console.log("handleConfirmBtnClick>>", pageNumber);
    console.log("deletedPages>>>", isPageDeleted);
    // setDeletedPages([...deletedPages, pageNumber]);
    handleAddRedaction(documentId, undefined, [{ pageNumber }]);
    setShowModal(false);
  };
  const handleRestoreBtnClick = () => {
    // const filteredPages = deletedPages.filter((page) => page !== pageNumber);
    // setDeletedPages([...filteredPages]);
    const redactionId = pageDeleteRedactions.find(
      (redaction) => redaction.pageNumber === pageNumber
    )?.id;
    if (redactionId) handleRemoveRedaction(redactionId);
  };

  const handleCancelBtnClick = () => {
    setShowModal(false);
  };
  return (
    <div>
      {!showModal && (
        <div className={classes.buttonWrapper}>
          <div className={classes.content}>
            <div className={classes.pageNumberWrapper}>
              <p className={classes.pageNumber}>
                <span>Page:</span>
                <span>1/2</span>
              </p>
            </div>
            {isPageDeleted ? (
              <LinkButton
                className={classes.deleteBtn}
                onClick={handleRestoreBtnClick}
                data-pageNumber={pageNumber}
              >
                Restore
              </LinkButton>
            ) : (
              <LinkButton
                className={classes.deleteBtn}
                onClick={handleDelete}
                data-pageNumber={pageNumber}
              >
                <DeleteIcon className={classes.deleteBtnIcon} width={"15px"} />
                Delete
              </LinkButton>
            )}
          </div>
        </div>
      )}
      {isPageDeleted && (
        <div>
          <div className={classes.overlay}></div>
          <div className={classes.overlayContent}>
            <DeleteIcon className={classes.overlayDeleteIcon} width={"15px"} />
            <p className={classes.overlayDeletedText}>Page Deleted</p>
          </div>
        </div>
      )}
      {showModal && (
        <div className={classes.deleteModal}>
          <div className={classes.contentWrapper}>
            <div className="govuk-form-group">
              <Select
                label={{
                  htmlFor: "select-redaction-type",
                  children: "Select Delete Reason",
                  className: classes.sortLabel,
                }}
                id="select-redaction-type"
                data-testid="select-redaction-type"
                value={deleteRedactionType}
                items={getMappedRedactionTypes()}
                formGroup={{
                  className: classes.select,
                }}
                onChange={(ev) => setDeleteRedactionType(ev.target.value)}
              />
            </div>
            <Button
              disabled={false}
              className={classes.redactButton}
              onClick={() => handleConfirmBtnClick()}
              data-testid="btn-redact"
              id="btn-redact"
            >
              Redact
            </Button>
            <LinkButton
              className={classes.cancelBtn}
              onClick={handleCancelBtnClick}
              data-pageNumber={pageNumber}
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      )}
    </div>
  );
};
