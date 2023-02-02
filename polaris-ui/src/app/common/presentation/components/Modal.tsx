import { useEffect } from "react";
import classes from "./Modal.module.scss";

type Props = {
  isVisible: boolean | undefined;
  type?: "data" | "alert";
  handleClose?: () => void;
};

export const Modal: React.FC<Props> = ({
  isVisible,
  children,
  type = "data",
  handleClose,
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

  useEffect(() => {
    // ... we also need to make sure the window scroll is reenabled if
    //  we are being unmounted before the the isVisible flag is seen to be
    //  false.
    return () => htmlElement.classList.remove(classes.stopHtmlScroll);
  }, [htmlElement.classList]);

  return !isVisible ? null : (
    <div
      data-testid="div-modal"
      className={classes.modal}
      onClick={handleClose}
    >
      <div
        className={
          type === "data"
            ? `${classes.modalContent} ${classes.modalContentData}`
            : classes.modalContent
        }
        onClick={(e) => e.stopPropagation()}
      >
        {type === "data" && (
          <div className={classes.closeContainer}>
            <button
              data-testid="btn-modal-close"
              type="button"
              className={classes.dataModalClose}
              aria-label="Close"
              onClick={handleClose}
            ></button>
          </div>
        )}
        {type === "alert" && (
          <header
            className="govuk-header"
            role="banner"
            data-module="govuk-header"
          >
            <div
              className={`govuk-header__container  ${classes.alertModalHeader}`}
            >
              <button
                data-testid="btn-modal-close"
                type="button"
                className={classes.alertModalClose}
                aria-label="Close"
                onClick={handleClose}
              ></button>
            </div>
          </header>
        )}
        <div className={classes.contentContainer}>{children}</div>
      </div>
    </div>
  );
};
