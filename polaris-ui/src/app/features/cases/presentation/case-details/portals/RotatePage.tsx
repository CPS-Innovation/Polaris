import { useState, useMemo, useRef } from "react";
import { LinkButton } from "../../../../../common/presentation/components";
import { DeleteModal } from "./DeleteModal";
import { ReactComponent as DeleteIcon } from "../../../../../common/presentation/svgs/deleteIcon.svg";
import { ReactComponent as RotateIcon } from "../../../../../common/presentation/svgs/rotateIcon.svg";
import { ReactComponent as PageIcon } from "../../../../../common/presentation/svgs/pageIcon.svg";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { IPageRotation } from "../../../domain/IPageRotation";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./RotatePage.module.scss";
import { transform } from "cypress/types/lodash";
type RotatePageProps = {
  documentId: string;
  pageNumber: number;
  totalPages: number;
  redactionTypesData: RedactionTypeData[];
  pageRotations: IPageRotation[];
  handleAddPageRotation: CaseDetailsState["handleAddPageRotation"];
  handleRemovePageRotation: CaseDetailsState["handleRemovePageRotation"];
};

export const RotatePage: React.FC<RotatePageProps> = ({
  documentId,
  pageNumber,
  totalPages,
  redactionTypesData,
  pageRotations,
  handleAddPageRotation,
  handleRemovePageRotation,
}) => {
  const trackEvent = useAppInsightsTrackEvent();

  const [deleteRedactionType, setDeleteRedactionType] = useState<string>("");
  const deleteButtonRef = useRef<HTMLButtonElement>(null);

  // console.log("totalPages>>", totalPages);
  // console.log("pageNumber>>", pageNumber);
  const pageRotateData = useMemo(() => {
    // console.log("pageRotations>>", pageRotations);
    return pageRotations.find((rotation) => rotation.pageNumber === pageNumber);
  }, [pageRotations, pageNumber]);
  // const [pageRotateData, setShowOverlay] = useState(pageRotateData?.);

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

  const handleRotateBtnClick = () => {
    handleAddPageRotation(documentId, [{ pageNumber, rotationAngle: 0 }]);
    // setShowOverlay(true);
  };
  const handleRotateLeft = () => {
    console.log("handleRotateLeft>>>");
    if (pageRotateData) {
      console.log("handleRotateLeft>>>", pageRotateData?.rotationAngle);
      handleAddPageRotation(documentId, [
        {
          pageNumber,
          rotationAngle: (pageRotateData?.rotationAngle - 90) % 360,
        },
      ]);
    }
    // trackEvent("Delete Page", {
    //   documentId: documentId,
    //   pageNumber: pageNumber,
    //   reason: redactionType.name,
    // });
  };
  const handleRotateRight = () => {
    if (pageRotateData)
      handleAddPageRotation(documentId, [
        {
          pageNumber,
          rotationAngle: (pageRotateData?.rotationAngle + 90) % 360,
        },
      ]);
    // trackEvent("Delete Page", {
    //   documentId: documentId,
    //   pageNumber: pageNumber,
    //   reason: redactionType.name,
    // });
  };

  const handleRestoreBtnClick = () => {
    if (pageRotateData) {
      handleRemovePageRotation(documentId, pageRotateData?.id);
      // trackEvent("Undo Delete Page", {
      //   documentId: documentId,
      //   pageNumber: pageNumber,
      // });
    }
  };

  return (
    <div>
      {
        <div className={classes.buttonWrapper}>
          <div className={classes.content}>
            <div className={classes.pageNumberWrapper}>
              <p className={classes.pageNumberText}>
                <span>Page:</span>
                <span className={classes.pageNumber}>
                  {pageNumber}/{totalPages}
                </span>
              </p>
            </div>
            {pageRotateData ? (
              <LinkButton
                className={classes.restoreBtn}
                onClick={handleRestoreBtnClick}
                data-pageNumber={pageNumber}
              >
                Cancel
              </LinkButton>
            ) : (
              <LinkButton
                ref={deleteButtonRef}
                className={classes.rotateBtn}
                onClick={handleRotateBtnClick}
                data-pageNumber={pageNumber}
              >
                <RotateIcon className={classes.rotateBtnIcon} />
                Rotate Page
              </LinkButton>
            )}
          </div>
        </div>
      }
      {pageRotateData && (
        <div>
          <div className={classes.overlay}></div>
          <div className={classes.overlayContent}>
            <div>
              <LinkButton
                className={classes.cancelBtn}
                onClick={handleRotateLeft}
                data-pageNumber={pageNumber}
              >
                Rotate page left
              </LinkButton>
              <PageIcon
                className={classes.overlayPageIcon}
                style={{
                  transform: `rotate(${pageRotateData?.rotationAngle}deg)`,
                }}
              />
              <LinkButton
                className={classes.cancelBtn}
                onClick={handleRotateRight}
                data-pageNumber={pageNumber}
              >
                Rotate page right
              </LinkButton>
            </div>
            <p className={classes.overlayMainText}>
              {`Rotate page ${pageRotateData?.rotationAngle}`}
            </p>
            <p className={classes.overlaySubText}>
              Click "save and submit" to submit changes to CMS
            </p>
            <LinkButton
              className={classes.cancelBtn}
              onClick={handleRestoreBtnClick}
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
