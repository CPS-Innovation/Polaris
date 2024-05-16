import React, { useState, useEffect } from "react";
import classes from "./RedactButton.module.scss";
import { Select, Button } from "../../../../../common/presentation/components";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";

type Props = {
  searchPIIData: {
    searchPIIOn: boolean;
    textContent: string;
    count: number;
    piiCategory: string;
  };
  redactionTypesData: RedactionTypeData[];
  onConfirm: (
    redactionType: { id: string; name: string },
    redactAll: boolean
  ) => void;
};

const getMappedRedactionTypes = (data: RedactionTypeData[]) => {
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
  searchPIIData,
}) => {
  const [redactionType, setRedactionType] = useState<string>("");
  useFocusTrap("#redact-modal");
  useLastFocus();

  useEffect(() => {
    if (searchPIIData.searchPIIOn) {
      const redactionType = getRedactionTypeFromPIIData();
      if (redactionType) setRedactionType(redactionType);
    }
  }, []);

  const getRedactionTypeFromPIIData = () => {
    return redactionTypesData.find(
      (type) => type.name === searchPIIData.piiCategory
    )?.name;
  };

  const handleClickRedact = (redactAll: boolean) => {
    if (redactionTypesData.length) {
      const selectedType = redactionTypesData.find(
        (type) => type.id === redactionType
      )!;
      onConfirm({ id: selectedType.id, name: selectedType.name }, redactAll);
      return;
    }
    onConfirm({ id: "", name: "" }, redactAll);
  };

  const handleClickIgnore = (ignoreAll: boolean) => {};
  return (
    <div
      id="redact-modal"
      className={
        redactionTypesData.length
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
      {searchPIIData.searchPIIOn && (
        <div className={classes.piiHeader}>
          <b>{`"${searchPIIData.textContent}" `}</b>
          <i className={classes.smallText}>appears</i>{" "}
          <b>{`${searchPIIData.count}`}</b>{" "}
          <i className={classes.smallText}>times in the document</i>
        </div>
      )}
      <div className={classes.contentWrapper}>
        {redactionTypesData.length > 0 && (
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
              onChange={(ev) => setRedactionType(ev.target.value)}
            />
          </div>
        )}

        <Button
          disabled={redactionTypesData.length ? !redactionType : false}
          className={classes.redactButton}
          onClick={() => handleClickRedact(false)}
          data-testid="btn-redact"
          id="btn-redact"
        >
          Redact
        </Button>

        {searchPIIData.searchPIIOn && (
          <>
            {searchPIIData.count > 0 && (
              <Button
                disabled={redactionTypesData.length ? !redactionType : false}
                className={classes.redactButton}
                onClick={() => handleClickRedact(true)}
                data-testid="btn-redact"
                id="btn-redact"
              >
                {`Redact all(${searchPIIData.count})`}
              </Button>
            )}
            <Button
              disabled={false}
              onClick={() => handleClickIgnore(false)}
              data-testid="btn-redact"
              id="btn-redact"
              className="govuk-button--secondary"
              name="secondary"
            >
              Ignore
            </Button>
            {searchPIIData.count > 0 && (
              <Button
                disabled={false}
                onClick={() => handleClickIgnore(true)}
                data-testid="btn-redact"
                id="btn-redact"
                className="govuk-button--secondary"
                name="secondary"
              >
                {`Ignore all(${searchPIIData.count})`}
              </Button>
            )}
          </>
        )}
      </div>
    </div>
  );
};
