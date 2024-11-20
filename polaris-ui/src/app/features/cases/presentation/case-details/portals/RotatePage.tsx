import { useMemo } from "react";
import { LinkButton } from "../../../../../common/presentation/components";
import { ReactComponent as RotateIcon } from "../../../../../common/presentation/svgs/rotateIcon.svg";
import { ReactComponent as PageIcon } from "../../../../../common/presentation/svgs/pageIcon.svg";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import { IPageRotation } from "../../../domain/IPageRotation";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import classes from "./RotatePage.module.scss";

type RotatePageProps = {
  documentId: string;
  pageNumber: number;
  totalPages: number;
  pageRotations: IPageRotation[];
  handleAddPageRotation: CaseDetailsState["handleAddPageRotation"];
  handleRemovePageRotation: CaseDetailsState["handleRemovePageRotation"];
};

export const RotatePage: React.FC<RotatePageProps> = ({
  documentId,
  pageNumber,
  totalPages,
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
    trackEvent("Rotate Page", {
      documentId: documentId,
      pageNumber: pageNumber,
    });
  };
  const handleRotateLeft = () => {
    if (pageRotateData) {
      const rotationAngle = (pageRotateData?.rotationAngle - 90) % 360;
      handleAddPageRotation(documentId, [
        {
          pageNumber,
          rotationAngle: rotationAngle,
        },
      ]);
      trackEvent("Rotate Page Left", {
        documentId: documentId,
        pageNumber: pageNumber,
        rotationAngle: rotationAngle,
      });
    }
  };
  const handleRotateRight = () => {
    if (pageRotateData) {
      const rotationAngle = (pageRotateData?.rotationAngle + 90) % 360;
      handleAddPageRotation(documentId, [
        {
          pageNumber,
          rotationAngle: rotationAngle,
        },
      ]);
      trackEvent("Rotate Page Right", {
        documentId: documentId,
        pageNumber: pageNumber,
        rotationAngle: rotationAngle,
      });
    }
  };

  const handleCancelBtnClick = () => {
    if (pageRotateData) {
      handleRemovePageRotation(documentId, pageRotateData?.id);
      trackEvent("Undo Rotate Page", {
        documentId: documentId,
        pageNumber: pageNumber,
      });
    }
  };

  return (
    <div>
      {
        <div className={classes.buttonWrapper}>
          <div className={classes.content}>
            <div className={classes.pageNumberWrapper}>
              <p
                className={classes.pageNumberText}
                data-testid={`rotate-page-number-text-${pageNumber}`}
              >
                <span>Page:</span>
                <span className={classes.pageNumber}>
                  {pageNumber}/{totalPages}
                </span>
              </p>
            </div>
            {pageRotateData && (
              <LinkButton
                className={classes.cancelBtn}
                onClick={handleCancelBtnClick}
                data-pageNumber={pageNumber}
                dataTestId={`btn-cancel-rotate-${pageNumber}`}
              >
                Cancel
              </LinkButton>
            )}
            {!pageRotateData && (
              <LinkButton
                className={classes.rotateBtn}
                onClick={handleRotateBtnClick}
                data-pageNumber={pageNumber}
                dataTestId={`btn-rotate-${pageNumber}`}
              >
                <RotateIcon className={classes.rotateBtnIcon} />
                Rotate page
              </LinkButton>
            )}
          </div>
        </div>
      }
      {pageRotateData && (
        <div>
          <div
            className={classes.overlay}
            data-testid={`rotate-page-overlay-${pageNumber}`}
          />
          <div
            className={classes.overlayContent}
            data-testid={`rotate-page-content-${pageNumber}`}
          >
            <div className={classes.rotateControlWrapper}>
              <LinkButton
                className={classes.rotateLeftBtn}
                onClick={handleRotateLeft}
                data-pageNumber={pageNumber}
                dataTestId={`rotate-page-left-btn-${pageNumber}`}
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
                dataTestId={`rotate-page-right-btn-${pageNumber}`}
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
            {pageRotateData?.rotationAngle && (
              <p
                className={classes.overlaySubText}
                data-testid="rotation-overlay-save-content"
              >
                Click <b>"Save all rotations"</b> to submit changes to CMS
              </p>
            )}
            <LinkButton
              className={classes.cancelBtn}
              onClick={handleCancelBtnClick}
              data-pageNumber={pageNumber}
              dataTestId="rotation-overlay-cancel-btn"
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      )}
    </div>
  );
};
