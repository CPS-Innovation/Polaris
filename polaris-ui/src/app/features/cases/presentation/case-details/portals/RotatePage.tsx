import { useMemo } from "react";
import { LinkButton } from "../../../../../common/presentation/components";
import { ReactComponent as RotateIcon } from "../../../../../common/presentation/svgs/rotateIcon.svg";
import { ReactComponent as PageIcon } from "../../../../../common/presentation/svgs/pageIcon.svg";

import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { IPageRotation } from "../../../domain/IPageRotation";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./RotatePage.module.scss";

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

  const pageRotateData = useMemo(() => {
    return pageRotations.find((rotation) => rotation.pageNumber === pageNumber);
  }, [pageRotations, pageNumber]);

  const handleRotateBtnClick = () => {
    handleAddPageRotation(documentId, [{ pageNumber, rotationAngle: 0 }]);
  };
  const handleRotateLeft = () => {
    if (pageRotateData) {
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
            <div className={classes.rotateControlWrapper}>
              <LinkButton
                className={classes.rotateLeftBtn}
                onClick={handleRotateLeft}
                data-pageNumber={pageNumber}
              >
                <div className={classes.rotateIconWrapper}>
                  <RotateIcon className={classes.rotateLeftIcon} />
                </div>
                <span className={classes.rotateBtnText}>Rotate page left</span>
              </LinkButton>
              <PageIcon
                className={classes.overlayPageIcon}
                style={{
                  transform: `rotate(${pageRotateData?.rotationAngle}deg)`,
                }}
              />
              <LinkButton
                className={classes.rotateRightBtn}
                onClick={handleRotateRight}
                data-pageNumber={pageNumber}
              >
                <span className={classes.rotateBtnText}>Rotate page right</span>
                <div className={classes.rotateIconWrapper}>
                  <RotateIcon className={classes.rotateRightIcon} />
                </div>
              </LinkButton>
            </div>
            <p className={classes.overlayMainText}>
              {`Rotate page ${pageRotateData?.rotationAngle}°`}
            </p>
            <p className={classes.overlaySubText}>
              Click <b>"save and submit"</b> to submit changes to CMS
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
