import { useState, useMemo } from "react";
import {
  Button,
  LinkButton,
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
  totalPages: number;
  redactionTypesData: RedactionTypeData[];
  pageDeleteRedactions: IPageDeleteRedaction[];
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: (id: string) => void;
};

export const DocumentButtons: React.FC<DocumentButtonsProps> = ({
  documentId,
  pageNumber,
  totalPages,
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
    const mappedRedactionType = redactionTypesData
      .filter((item) => item.isDeletedPage)
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
  const [deleteRedactionType, setDeleteRedactionType] = useState<string>("");
  const handleRotate = (e: any) => {
    console.log("handleRotate>>", pageNumber);
  };
  const handleDelete = () => {
    console.log("handleDelete>>", pageNumber);
    setShowModal(true);
  };

  const handleConfirmBtnClick = () => {
    const redactionType = redactionTypesData.find(
      (type) => type.id === deleteRedactionType
    )!;
    handleAddRedaction(documentId, undefined, [{ pageNumber, redactionType }]);

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
                <span>
                  {pageNumber}/{totalPages}
                </span>
              </p>
            </div>
            {isPageDeleted ? (
              <LinkButton
                className={classes.deleteBtn}
                onClick={handleRestoreBtnClick}
                data-pageNumber={pageNumber}
              >
                Cancel
              </LinkButton>
            ) : (
              <LinkButton
                className={classes.deleteBtn}
                onClick={handleDelete}
                data-pageNumber={pageNumber}
                disabled={
                  totalPages === 1 ||
                  pageDeleteRedactions.length === totalPages - 1
                }
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
            <p className={classes.overlayMainText}>
              Page selected for deletion
            </p>
            <p className={classes.overlaySubText}>
              Click "save and submit" to remove the page from the document
            </p>
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
