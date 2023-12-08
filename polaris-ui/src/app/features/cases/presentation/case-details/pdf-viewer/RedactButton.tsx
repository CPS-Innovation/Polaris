import React, { useState } from "react";
import classes from "./RedactButton.module.scss";
import { Select } from "../../../../../common/presentation/components";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import { RedactionType } from "../../../domain/redactionLog/RedactionType";
import { FEATURE_FLAG_REDACTION_LOG } from "../../../../../config";

type Props = {
  onConfirm: (redactionType: { id: string; name: RedactionType }) => void;
};

const redactionTypeOptions: { children: string; value: RedactionType | "" }[] =
  [
    {
      children: "-- select redaction type --",
      value: "",
    },
    {
      children: "Address",
      value: "Address",
    },
    {
      children: "Date of birth",
      value: "Date of birth",
    },
    {
      children: "Named individual",
      value: "Named individual",
    },
  ];

export const RedactButton: React.FC<Props> = ({ onConfirm }) => {
  const [redactionType, setRedactionType] = useState<RedactionType | "">("");
  useFocusTrap("#redact-modal");
  useLastFocus();

  const handleClickRedact = () => {
    if (FEATURE_FLAG_REDACTION_LOG) {
      onConfirm({ id: "1", name: redactionType });
      return;
    }
    onConfirm({ id: "", name: "" });
  };
  return (
    <div
      id="redact-modal"
      className={
        FEATURE_FLAG_REDACTION_LOG
          ? classes.redactionModal
          : classes.redactBtnModal
      }
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="redact-modal-label"
      aria-describedby="redact-modal-description"
    >
      <span id="redact-modal-label" className={classes.modalLabel}>
        Redaction modal
      </span>
      <span id="redact-modal-description" className={classes.modalDescription}>
        A modal with a redact button to help user to redact selected text
      </span>
      {FEATURE_FLAG_REDACTION_LOG && (
        <div className="govuk-form-group">
          <Select
            label={{
              htmlFor: "select-redaction-type",
              children: "Select Redaction Type",
              className: classes.sortLabel,
            }}
            id="select-redaction-type"
            data-testid="select-redaction-type"
            value={redactionType}
            items={redactionTypeOptions}
            formGroup={{
              className: classes.select,
            }}
            onChange={(ev) =>
              setRedactionType(ev.target.value as RedactionType)
            }
          />
        </div>
      )}
      <button
        disabled={FEATURE_FLAG_REDACTION_LOG ? !redactionType : false}
        className={classes.redactButton}
        onClick={handleClickRedact}
        data-testid="btn-redact"
        id="btn-redact"
      >
        Redact
      </button>
    </div>
  );
};
