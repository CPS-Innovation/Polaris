import { useState, useEffect, useRef, useCallback } from "react";
import { ReactComponent as CloseIcon } from "../svgs/closeIconBold.svg";
import { ReactComponent as HelpIcon } from "../svgs/help.svg";
import { LinkButton } from "../components/LinkButton";
import { useFocusTrap } from "../../hooks/useFocusTrap";
import classes from "./Guidance.module.scss";
export type GuidanceProps = {
  name: string;
  className?: string;
  dataTestId?: string;
  ariaLabel: string;
  ariaDescription: string;
};

export const Guidance: React.FC<GuidanceProps> = ({
  name,
  className,
  children,
  dataTestId = "guidance-btn",
  ariaLabel,
  ariaDescription,
}) => {
  const guidanceBtnRef = useRef<HTMLButtonElement | null>(null);
  const panelRef = useRef<HTMLDivElement | null>(null);
  const [buttonOpen, setButtonOpen] = useState(false);
  const buttonOpenRef = useRef<boolean>(false);
  useFocusTrap("#guidance-panel");

  useEffect(() => {
    buttonOpenRef.current = buttonOpen;
  }, [buttonOpen]);

  const handleOutsideClick = useCallback((event: MouseEvent) => {
    if (panelRef.current && event.target && buttonOpenRef.current) {
      if (!panelRef.current?.contains(event.target as Node)) {
        event.stopPropagation();
        setButtonOpen(false);
        guidanceBtnRef.current?.focus();
      }
    }
  }, []);

  useEffect(() => {
    document.addEventListener("click", handleOutsideClick);
    return () => {
      document.removeEventListener("click", handleOutsideClick);
    };
  }, []);

  return (
    <div className={`${classes.guidanceButtonWrapper} ${className}`}>
      <div className={classes.helpButtonWrapper}>
        <LinkButton
          type="button"
          dataTestId={dataTestId}
          ref={guidanceBtnRef}
          ariaLabel={ariaLabel}
          ariaExpanded={buttonOpen}
          className={`${classes.guidanceButton} `}
          onClick={() => {
            setButtonOpen((buttonOpen) => !buttonOpen);
          }}
        >
          <HelpIcon className={classes.helpIcon} />
          {name}
        </LinkButton>
      </div>

      {buttonOpen && (
        <div
          className={classes.panel}
          ref={panelRef}
          id="guidance-panel"
          data-testid={`${dataTestId}-panel`}
          role="dialog"
          aria-modal="true"
          aria-labelledby="guidance-modal-label"
          aria-describedby="guidance-modal-description"
        >
          <span id="guidance-modal-label" className={classes.modalLabel}>
            {ariaLabel}
          </span>
          <span
            id="guidance-modal-description"
            className={classes.modalDescription}
          >
            {ariaDescription}
          </span>
          <button
            data-testid="btn-modal-close"
            type="button"
            className={classes.guidancePanelCloseBtn}
            aria-label="close guidance"
            onClick={() => {
              setButtonOpen(false);
              guidanceBtnRef.current?.focus();
            }}
          >
            <CloseIcon height={"1.5625rem"} width={"1.5625rem"} />
          </button>
          {children}
        </div>
      )}
    </div>
  );
};
