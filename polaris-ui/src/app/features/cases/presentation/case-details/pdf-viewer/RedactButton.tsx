import React, { useState } from "react";
import classes from "./RedactButton.module.scss";
import { Select, Button } from "../../../../../common/presentation/components";
import { useFocusTrap } from "../../../../../common/hooks/useFocusTrap";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { PIIRedactionStatus } from "../../../domain/NewPdfHighlight";

type Props = {
  searchPIIData?: {
    textContent: string;
    count: number;
    isSearchPIIDefaultOptionOn: boolean;
  };
  redactionTypesData: RedactionTypeData[];
  onConfirm: (
    redactionType: { id: string; name: string },
    actionType: PIIRedactionStatus | "redact"
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

  const handleSearchPIIBtnClick = (actionType: PIIRedactionStatus) => {
    onConfirm({ id: "", name: "" }, actionType);
  };

  const handleRedactBtnClick = () => {
    if (redactionTypesData.length) {
      const selectedType = redactionTypesData.find(
        (type) => type.id === redactionType
      )!;
      onConfirm({ id: selectedType.id, name: selectedType.name }, "redact");
      return;
    }
    // this is fallback for handling redaction without redactionLog
    onConfirm({ id: "", name: "" }, "redact");
  };

  return (
    <div
      id="redact-modal"
      className={
        redactionTypesData.length || searchPIIData
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
      {searchPIIData && (
        <div className={classes.piiHeader}>
          <b>{`"${searchPIIData.textContent}" `}</b>
          <i className={classes.smallText}>appears</i>{" "}
          <b>{`${searchPIIData.count}`}</b>{" "}
          <i className={classes.smallText}>times in the document</i>
        </div>
      )}
      <div className={classes.contentWrapper}>
        {!searchPIIData && redactionTypesData.length > 0 && (
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
        {!searchPIIData && (
          <Button
            disabled={redactionTypesData.length ? !redactionType : false}
            className={classes.redactButton}
            onClick={() => handleRedactBtnClick()}
            data-testid="btn-redact"
            id="btn-redact"
          >
            Redact
          </Button>
        )}
        {searchPIIData && (
          <>
            <Button
              disabled={false}
              onClick={() => handleSearchPIIBtnClick("accepted")}
              data-testid="btn-accept"
              id="btn-accept"
            >
              Accept
            </Button>
            {searchPIIData.count > 1 && (
              <Button
                disabled={false}
                onClick={() => handleSearchPIIBtnClick("acceptedAll")}
                data-testid="btn-accept-all"
                id="btn-accept-all"
              >
                {`Accept all(${searchPIIData.count})`}
              </Button>
            )}
            {!searchPIIData.isSearchPIIDefaultOptionOn && (
              <Button
                disabled={false}
                onClick={() => handleSearchPIIBtnClick("ignored")}
                data-testid="btn-ignore"
                id="btn-ignore"
                className="govuk-button--secondary"
                name="secondary"
              >
                Ignore
              </Button>
            )}
            {searchPIIData.count > 1 &&
              !searchPIIData.isSearchPIIDefaultOptionOn && (
                <Button
                  disabled={false}
                  onClick={() => handleSearchPIIBtnClick("ignoredAll")}
                  data-testid="btn-ignore-all"
                  id="btn-ignore-all"
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
