import { useEffect, useRef, useCallback } from "react";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import {
  Button,
  LinkButton,
  Select,
} from "../../../../../common/presentation/components";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import classes from "./DeleteModal.module.scss";
export type DeleteModalProps = {
  deleteRedactionType: string;
  redactionTypeValues: {
    value: string;
    children: string;
  }[];
  parentButtonRef: React.RefObject<HTMLButtonElement>;
  handleConfirmRedaction: () => void;
  handleRedactionTypeSelection: (value: string) => void;
  handleCancelRedaction: () => void;
};
export const DeleteModal: React.FC<DeleteModalProps> = ({
  deleteRedactionType,
  redactionTypeValues,
  parentButtonRef,
  handleConfirmRedaction,
  handleCancelRedaction,
  handleRedactionTypeSelection,
}) => {
  const panelRef = useRef<HTMLDivElement | null>(null);
  useFocusTrap("#delete-page-modal");
  useLastFocus();
  const handleOutsideClick = useCallback((event: MouseEvent) => {
    if (
      event.target === parentButtonRef.current ||
      parentButtonRef.current?.contains(event.target as Node)
    ) {
      return;
    }
    if (
      panelRef.current &&
      event.target &&
      !panelRef.current?.contains(event.target as Node)
    ) {
      handleCancelRedaction();
      event.stopPropagation();
    }
  }, []);
  const keyDownHandler = useCallback((event: KeyboardEvent) => {
    if (event.code === "Escape" && panelRef.current) {
      handleCancelRedaction();
      parentButtonRef?.current?.focus();
    }
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyDownHandler);
    document.addEventListener("click", handleOutsideClick);
    return () => {
      window.removeEventListener("keydown", keyDownHandler);
      document.removeEventListener("click", handleOutsideClick);
    };
  }, []);
  return (
    <div
      id="delete-page-modal"
      data-testid="delete-page-modal"
      className={classes.deleteModal}
      ref={panelRef}
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="delete-page-modal-label"
      aria-describedby="delete-page-modal-description"
    >
      <span id="delete-page-modal-label" className={classes.visuallyHidden}>
        delete page modal
      </span>
      <span
        id="delete-page-modal-description"
        className={classes.visuallyHidden}
      >
        A modal with a delete reason selection and a redact button to help user
        confirm the deletion of the page
      </span>
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
            items={redactionTypeValues}
            formGroup={{
              className: classes.select,
            }}
            onChange={(ev) => handleRedactionTypeSelection(ev.target.value)}
          />
        </div>
        <Button
          disabled={!deleteRedactionType}
          className={classes.redactButton}
          onClick={handleConfirmRedaction}
          data-testid="delete-page-modal-btn-redact"
          id="delete-page-modal-btn-redact"
        >
          Redact
        </Button>
        <LinkButton
          className={classes.cancelBtn}
          onClick={handleCancelRedaction}
          dataTestId="delete-page-modal-btn-cancel"
        >
          Cancel
        </LinkButton>
      </div>
    </div>
  );
};
