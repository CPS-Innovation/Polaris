import { useEffect } from "react";
import { useFocusTrap } from "../../hooks/useFocusTrap";
import { useLastFocus } from "../../hooks/useLastFocus";
import { ReactComponent as CloseIcon } from "../../presentation/svgs/closeIconBold.svg";
import classes from "./Modal.module.scss";

type Props = {
  isVisible: boolean | undefined;
  type?: "data" | "alert";
  ariaLabel: string;
  ariaDescription: string;
  handleClose?: () => void;
  className?: string;
  defaultLastFocusId?: string;
  children: React.ReactNode;
};

export const Modal: React.FC<Props> = ({
  isVisible,
  children,
  ariaLabel,
  ariaDescription,
  type = "data",
  handleClose,
  className,
  defaultLastFocusId,
}) => {
  // govuk styling seems to lead to the html element being the thing
  //  that scrolls rather than body.  We want to prevent the main content
  //  scrolling when we scroll the modal content.
  //  todo: not especially pure/functional!
  const htmlElement = document.getElementsByTagName("html")[0];
  if (isVisible) {
    htmlElement.classList.add(classes.stopHtmlScroll);
  } else {
    // We need to reenable the scrolling the behaviour for the window
    //  if we are hiding the modal. But see following comment....
    htmlElement.classList.remove(classes.stopHtmlScroll);
  }
  useLastFocus(defaultLastFocusId);
  useFocusTrap();
  useEffect(() => {
    // ... we also need to make sure the window scroll is reenabled if
    //  we are being unmounted before the the isVisible flag is seen to be
    //  false.
    return () => htmlElement.classList.remove(classes.stopHtmlScroll);
  }, [htmlElement.classList]);

  if (!isVisible) {
    return null;
  }

  const handleCloseClickHandler = () => {
    if (handleClose) {
      handleClose();
    }
  };

  return (
    <>
      <div
        className={classes.backDrop}
        role="presentation"
        onClick={handleCloseClickHandler}
      />
      <div
        id={"modal"}
        data-testid="div-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-label"
        aria-describedby="modal-description"
        className={
          type === "data"
            ? `${classes.modalContent} ${classes.modalContentData} ${className}`
            : `${classes.modalContent} ${className}`
        }
      >
        <span id="modal-label" className={classes.modalLabel}>
          {ariaLabel}
        </span>
        <span id="modal-description" className={classes.modalDescription}>
          {ariaDescription}
        </span>
        <div
          role="presentation"
          onKeyDown={(e: React.KeyboardEvent<HTMLDivElement>) => {
            if (e.code === "Escape") {
              handleCloseClickHandler();
            }
          }}
        >
          {type === "data" && handleClose && (
            <div className={classes.closeContainer}>
              <button
                data-testid="btn-modal-close"
                type="button"
                className={classes.dataModalClose}
                aria-label="close modal"
                onClick={handleCloseClickHandler}
              >
                <CloseIcon height={"2.5rem"} width={"2.5rem"} />
              </button>
            </div>
          )}
          {type === "alert" && (
            <div
              className={`govuk-header ${classes.modalHeader}`}
              data-module="govuk-header"
            >
              {handleClose && (
                <div
                  className={`govuk-header__container  ${classes.alertModalHeader}`}
                >
                  <button
                    data-testid="btn-modal-close"
                    type="button"
                    className={classes.alertModalClose}
                    aria-label="close modal"
                    onClick={handleCloseClickHandler}
                  >
                    <CloseIcon height={"1.5625rem"} width={"1.5625rem"} />
                  </button>
                </div>
              )}
            </div>
          )}
          <div>{children}</div>
        </div>
      </div>
    </>
  );
};
