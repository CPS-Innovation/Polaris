import { useState, useEffect } from "react";
import { Checkboxes } from "../../../../../common/presentation/components/Checkboxes";
import { Radios } from "../../../../../common/presentation/components/Radios";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { UseFormRegister } from "react-hook-form";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";
import { ErrorState } from "./RedactionLogContent";
import classes from "./UnderOverRedactionContent.module.scss";

type UnderOverRedactionContentProps = {
  redactionTypes: RedactionTypeData[];
  showErrors?: boolean;
  register: UseFormRegister<UnderRedactionFormData>;
  getValues: any;
  watch: any;
  errorState: ErrorState;
};

export const UnderOverRedactionContent: React.FC<
  UnderOverRedactionContentProps
> = ({ redactionTypes, register, getValues, watch, errorState }) => {
  const [toggleReDraw, setToggleReDraw] = useState(false);
  const redactionTypeCheckboxItems = (
    category: "underRedaction" | "overRedaction"
  ) => {
    return redactionTypes.map((type) => ({
      checked: getValues(`${category}-type-${type.id}`),
      children: type.name,
      id: `checkbox-${category}-type-${type.id}`,
      value: type.id,
      ...register(`${category}-type-${type.id}`, {
        validate: () => errorState[category] !== true,
      }),
    }));
  };

  const redactionCategoryCheckboxItem = [
    {
      checked: getValues(`underRedaction`),
      children: "Under Redaction",
      hint: {
        children: "Returned to Investigative Agency for correction",
      },
      id: "checkbox-under-redaction",
      ...register(`underRedaction`, {
        validate: () => errorState.category !== true,
      }),
      conditional: {
        children: [
          <Checkboxes
            key={"under-redaction-types"}
            errorMessage={
              errorState.underRedaction
                ? {
                    children: "Select an under redaction type",
                  }
                : undefined
            }
            fieldset={{
              legend: {
                children: "Types of redactions",
              },
            }}
            id="checkboxes-under-redaction-types"
            data-testid="checkboxes-under-redaction-types"
            name="under-redaction-types"
            items={redactionTypeCheckboxItems("underRedaction")}
            className={`govuk-checkboxes--small ${classes.redactionTypes} `}
          />,
        ],
      },
    },
    {
      checked: getValues(`overRedaction`),
      children: "Over Redaction",
      id: "checkbox-over-redaction",
      ...register(`overRedaction`, {
        validate: () => errorState.category !== true,
      }),
      conditional: {
        children: [
          <Radios
            key={"return-to-ia"}
            value={getValues(`returnToIA`)}
            name="radio-return-to-investigative-agency"
            items={[
              {
                ...register(`returnToIA`),
                children: "Returned to Investigative Agency for correction",
                value: "true",
              },
              {
                ...register(`returnToIA`),
                children: "Returned to CPS colleague for correction",
                value: "false",
              },
            ]}
          />,
          <Checkboxes
            key={"over-redaction-types"}
            errorMessage={
              errorState.overRedaction
                ? {
                    children: "Select an over redaction type",
                  }
                : undefined
            }
            fieldset={{
              legend: {
                children: "Types of redactions",
              },
            }}
            id="checkboxes-over-redaction-types"
            data-testid="checkboxes-over-redaction-types"
            name="over-redaction-types"
            items={redactionTypeCheckboxItems("overRedaction")}
            className={`govuk-checkboxes--small ${classes.redactionTypes} `}
          />,
        ],
      },
    },
  ];

  useEffect(() => {
    const subscription = watch(() => {
      setToggleReDraw((state) => !state);
    });
    return () => subscription.unsubscribe();
  }, [watch]);

  return (
    <div
      className={classes.underOverRedactionContent}
      data-testid="under-over-redaction-content"
    >
      <section>
        <Checkboxes
          errorMessage={
            errorState.category
              ? {
                  children: "Select a redaction type",
                }
              : undefined
          }
          fieldset={{
            legend: {
              children: "Confirm the redaction type",
            },
          }}
          id="checkboxes-under-over"
          data-testid="checkboxes-under-over"
          name="redaction-category"
          items={redactionCategoryCheckboxItem}
          className={`govuk-checkboxes--large ${classes.underOverCheckBoxes}`}
        />
      </section>
    </div>
  );
};
