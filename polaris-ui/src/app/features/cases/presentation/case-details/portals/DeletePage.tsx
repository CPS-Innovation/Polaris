import { useState, useMemo, useRef, useEffect } from "react";
import { LinkButton } from "../../../../../common/presentation/components";
import { DeleteModal } from "./DeleteModal";
import { ReactComponent as DeleteIcon } from "../../../../../common/presentation/svgs/deleteIcon.svg";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { IPageDeleteRedaction } from "../../../domain/IPageDeleteRedaction";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./DeletePage.module.scss";
type DeletePageProps = {
  documentId: string;
  pageNumber: number;
  totalPages: number;
  redactionTypesData: RedactionTypeData[];
  pageDeleteRedactions: IPageDeleteRedaction[];
  handleAddRedaction: CaseDetailsState["handleAddRedaction"];
  handleRemoveRedaction: (id: string) => void;
};

export const DeletePage: React.FC<DeletePageProps> = ({
  documentId,
  pageNumber,
  totalPages,
  redactionTypesData,
  pageDeleteRedactions,
  handleAddRedaction,
  handleRemoveRedaction,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [showModal, setShowModal] = useState(false);
  const [deleteRedactionType, setDeleteRedactionType] = useState<string>("");
  const deleteButtonRef = useRef<HTMLButtonElement>(null);

  const isPageDeleted = useMemo(() => {
    return pageDeleteRedactions.find(
      (redaction) => redaction.pageNumber === pageNumber
    );
  }, [pageDeleteRedactions, pageNumber]);

  //adding/removing any anchor links in the unsaved deleted page as tabbable, for accessibility
  useEffect(() => {
    const pages = document.querySelectorAll(".page");
    const links = pages[pageNumber - 1]?.querySelectorAll("a");

    links.forEach((link) => {
      if (isPageDeleted) {
        link.setAttribute("tabindex", "-1");
        return;
      }
      link.setAttribute("tabindex", "0");
    });
  }, [isPageDeleted, pageNumber]);

  const mappedRedactionTypeValues = useMemo(() => {
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
  }, [redactionTypesData]);

  const handleDelete = () => {
    setShowModal(true);
  };
  const handleConfirmRedaction = () => {
    setShowModal(false);
    const redactionType = redactionTypesData.find(
      (type) => type.id === deleteRedactionType
    )!;
    handleAddRedaction(documentId, undefined, [{ pageNumber, redactionType }]);
    trackEvent("Delete Page", {
      documentId: documentId,
      pageNumber: pageNumber,
      reason: redactionType.name,
    });
  };
  const handleRedactionTypeSelection = (value: string) => {
    setDeleteRedactionType(value);
  };
  const handleRestoreBtnClick = () => {
    const redactionId = pageDeleteRedactions.find(
      (redaction) => redaction.pageNumber === pageNumber
    )?.id;
    if (redactionId) {
      handleRemoveRedaction(redactionId);
      trackEvent("Undo Delete Page", {
        documentId: documentId,
        pageNumber: pageNumber,
      });
      setDeleteRedactionType("");
    }
  };
  const handleCancelRedaction = () => {
    setShowModal(false);
    setDeleteRedactionType("");
  };

  return (
    <div>
      {
        <div className={classes.buttonWrapper}>
          <div className={classes.content}>
            <div className={classes.pageNumberWrapper}>
              <p
                className={classes.pageNumberText}
                data-testId={`page-number-text-${pageNumber}`}
              >
                <span>Page:</span>
                <span className={classes.pageNumber}>
                  {pageNumber}/{totalPages}
                </span>
              </p>
            </div>
            {isPageDeleted ? (
              <LinkButton
                className={classes.restoreBtn}
                onClick={handleRestoreBtnClick}
                data-pageNumber={pageNumber}
                dataTestId={`btn-cancel-${pageNumber}`}
              >
                Cancel
              </LinkButton>
            ) : (
              <LinkButton
                ref={deleteButtonRef}
                className={classes.deleteBtn}
                onClick={handleDelete}
                data-pageNumber={pageNumber}
                disabled={
                  totalPages === 1 ||
                  pageDeleteRedactions.length === totalPages - 1
                }
                dataTestId={`btn-delete-${pageNumber}`}
              >
                <DeleteIcon className={classes.deleteBtnIcon} />
                Delete
              </LinkButton>
            )}
          </div>
        </div>
      }
      {isPageDeleted && (
        <div>
          <div
            className={classes.overlay}
            data-testid={`delete-page-overlay-${pageNumber}`}
          ></div>
          <div
            className={classes.overlayContent}
            data-testid={`delete-page-content-${pageNumber}`}
          >
            <DeleteIcon className={classes.overlayDeleteIcon} />
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
        <DeleteModal
          parentButtonRef={deleteButtonRef}
          deleteRedactionType={deleteRedactionType}
          redactionTypeValues={mappedRedactionTypeValues}
          handleConfirmRedaction={handleConfirmRedaction}
          handleRedactionTypeSelection={handleRedactionTypeSelection}
          handleCancelRedaction={handleCancelRedaction}
        />
      )}
    </div>
  );
};
