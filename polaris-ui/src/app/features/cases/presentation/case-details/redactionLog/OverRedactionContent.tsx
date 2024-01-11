import { useMemo, useState } from "react";
import classes from "./OverRedactionContent.module.scss";
// import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { ReactComponent as DocIcon } from "../../../../../common/presentation/svgs/doc.svg";
import {
  Checkboxes,
  CheckboxesProps,
} from "../../../../../common/presentation/components/Checkboxes";
import { Select } from "../../../../../common/presentation/components";
import { useForm, Controller, FieldErrors } from "react-hook-form";
import { RedactionCategory } from "../../../domain/redactionLog/RedactionCategory";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";

type OverRedactionContentProps = {
  redactionTypes: RedactionTypeData[];
  savedRedactionTypes?: RedactionTypeData[];
};

export const OverRedactionContent: React.FC<OverRedactionContentProps> = ({
  redactionTypes,
  savedRedactionTypes,
}) => {
  type OverUnderState = {
    underRedaction: {
      returnToIA: boolean;
      checked: boolean;
      redactionTypes: { id: string; checked: boolean }[];
    };
    overRedaction: {
      returnToIA: boolean;
      checked: boolean;
      redactionTypes: { id: string; checked: boolean }[];
    };
  };
  const [overUnderState, setOverUnderState] = useState<OverUnderState>({
    underRedaction: {
      returnToIA: true,
      checked: false,
      redactionTypes: [],
    },
    overRedaction: {
      returnToIA: true,
      checked: false,
      redactionTypes: [],
    },
  });

  const handleUnderOverSelection = (
    ev: React.ChangeEvent<HTMLInputElement>
  ) => {
    console.log("ev.currentTarget.id>>", ev.currentTarget.id);
    console.log("ev.target.checked>>", ev.target.checked);
    switch (ev.currentTarget.id) {
      case "1":
        setOverUnderState({
          ...overUnderState,
          underRedaction: {
            ...overUnderState.underRedaction,
            checked: ev.target.checked,
          },
        });
        break;
      case "2":
        setOverUnderState({
          ...overUnderState,
          overRedaction: {
            ...overUnderState.overRedaction,
            checked: ev.target.checked,
          },
        });
        break;
    }
  };

  const updateState = (
    category: "underRedaction" | "overRedaction",
    id: string,
    checked: boolean
  ) => {
    setOverUnderState((prevState) => {
      // Create a new object with the updated values
      const updatedState = {
        ...prevState,
        [category]: {
          ...prevState[category],
          types: prevState[category].redactionTypes.map((item) => {
            if (item.id === id) {
              return { ...item, checked: checked };
            }
            return item;
          }),
        },
      };

      return updatedState;
    });
  };

  const handleUnderRedactionTypeSelection = (
    ev: React.ChangeEvent<HTMLInputElement>
  ) => {
    updateState("underRedaction", ev.currentTarget.id, ev.target.checked);
  };

  const handleOverRedactionTypeSelection = (
    ev: React.ChangeEvent<HTMLInputElement>
  ) => {
    updateState("overRedaction", ev.currentTarget.id, ev.target.checked);
  };

  console.log("redactionTypes>>", redactionTypes);

  const underRedactionCheckboxItem = [
    {
      checked: overUnderState.underRedaction.checked,
      children: "Under Redaction",
      id: "1",
      value: "under redaction",
    } as CheckboxesProps["items"],
  ];

  const overRedactionCheckboxItem = [
    {
      checked: overUnderState.overRedaction.checked,
      children: "Over Redaction",
      id: "2",
      value: "Over redaction",
    } as CheckboxesProps["items"],
  ];

  const returnToIACheckboxItem = [
    {
      checked: overUnderState.overRedaction.returnToIA,
      children: "Return to Investigative Agency",
      id: "1",
      value: "1",
    } as CheckboxesProps["items"],
  ];

  const redactionTypeCheckboxItems = (
    category: "underRedaction" | "overRedaction"
  ) => {
    return redactionTypes.map(
      (type) =>
        ({
          checked: overUnderState[category].redactionTypes.find(
            ({ id }) => id === type.id
          )?.checked,
          children: type.name,
          id: type.id,
          value: type.id,
        } as CheckboxesProps["items"])
    );
  };
  return (
    <div className={classes.underRedactionContent}>
      <div className={classes.selectInputWrapper}>
        <section className={classes.underRedactionSection}>
          <Checkboxes
            fieldset={{
              legend: {
                children: "Confirm the redaction type",
              },
            }}
            data-testid="checkboxes-under-over"
            name="redactionCategory"
            items={underRedactionCheckboxItem}
            className="govuk-checkboxes--large"
            onChange={handleUnderOverSelection}
          />
          <span>Returned to Investigative Agency for correction</span>

          <div className={classes.redactionTypes}>
            <Checkboxes
              fieldset={{
                legend: {
                  children: "Types of redactions",
                },
              }}
              data-testid="checkboxes-under-redaction-types"
              name="redaction types"
              items={redactionTypeCheckboxItems("overRedaction")}
              className={`govuk-checkboxes--small ${classes.redactionTypes} `}
              onChange={handleOverRedactionTypeSelection}
            />
          </div>
        </section>

        <section className={classes.overRedactionSection}>
          <Checkboxes
            data-testid="checkboxes-under-over"
            name="redactionCategory"
            items={overRedactionCheckboxItem}
            className="govuk-checkboxes--large"
            onChange={handleUnderOverSelection}
          />
          <Checkboxes
            data-testid="checkboxes-under-return-to-IA"
            name="Return to Investigative Agency"
            items={returnToIACheckboxItem}
            className="govuk-checkboxes--large"
            onChange={handleUnderOverSelection}
          />
          <div className={classes.redactionTypes}>
            <Checkboxes
              fieldset={{
                legend: {
                  children: "Types of redactions",
                },
              }}
              data-testid="checkboxes-under-redaction-types"
              name="redaction types"
              items={redactionTypeCheckboxItems("underRedaction")}
              className={`govuk-checkboxes--small ${classes.redactionTypes} `}
              onChange={handleUnderRedactionTypeSelection}
            />
          </div>
        </section>
      </div>
    </div>
  );
};
