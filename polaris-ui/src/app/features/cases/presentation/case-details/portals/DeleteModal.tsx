import { useEffect, useRef, useCallback } from "react";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import {
  Button,
  LinkButton,
  Select,
} from "../../../../../common/presentation/components";
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
  useFocusTrap("#delete-modal");
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
    <div id="delete-modal" className={classes.deleteModal} ref={panelRef}>
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
          data-testid="btn-redact"
          id="btn-redact"
        >
          Redact
        </Button>
        <LinkButton
          className={classes.cancelBtn}
          onClick={handleCancelRedaction}
        >
          Cancel
        </LinkButton>
      </div>
    </div>
  );
};
