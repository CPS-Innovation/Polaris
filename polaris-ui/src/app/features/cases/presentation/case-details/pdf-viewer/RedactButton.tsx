import React, { useState } from "react";
import classes from "./RedactButton.module.scss";
import { Select } from "../../../../../common/presentation/components";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import { RedactionType } from "../../../domain/redactionLog/RedactionType";
import { FEATURE_FLAG_REDACTION_LOG } from "../../../../../config";
import { RedactionTypes } from "../../../domain/redactionLog/RedactionLogData";

type Props = {
  redactionTypesData: RedactionTypes[];
  onConfirm: (redactionType: { id: string; name: RedactionType }) => void;
};

const getMappedRedactionTypes = (data: RedactionTypes[]) => {
  const defaultOption = {
    value: "",
    children: "-- Please select --",
    disabled: true,
  };
  const mappedRedactionType = data.map((item) => ({
    value: item.id,
    children: item.name,
  }));

  return [defaultOption, ...mappedRedactionType];
};

export const RedactButton: React.FC<Props> = ({
  onConfirm,
  redactionTypesData,
}) => {
  const [redactionType, setRedactionType] = useState<RedactionType | "">("");
  useFocusTrap("#redact-modal");
  useLastFocus();

  const handleClickRedact = () => {
    if (FEATURE_FLAG_REDACTION_LOG) {
      const selectedType = redactionTypesData.find(
        (type) => type.id === redactionType
      )!;
      onConfirm({ id: selectedType.id, name: selectedType.name });
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
            items={getMappedRedactionTypes(redactionTypesData)}
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
