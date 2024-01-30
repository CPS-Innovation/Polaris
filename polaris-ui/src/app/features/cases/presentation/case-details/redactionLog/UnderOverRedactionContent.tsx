import { useState, useEffect, useCallback } from "react";
import { Checkboxes } from "../../../../../common/presentation/components/Checkboxes";
import { Radios } from "../../../../../common/presentation/components/Radios";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";
import { UseFormRegister } from "react-hook-form";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";
import classes from "./UnderOverRedactionContent.module.scss";

type UnderOverRedactionContentProps = {
  redactionTypes: RedactionTypeData[];
  showErrors?: boolean;
  register: UseFormRegister<UnderRedactionFormData>;
  getValues: any;
  watch: any;
  trigger: any;
  isSubmitted: boolean;
};

export const UnderOverRedactionContent: React.FC<
  UnderOverRedactionContentProps
> = ({ redactionTypes, register, getValues, watch, trigger, isSubmitted }) => {
  type ErrorState = {
    category: boolean;
    underRedaction: boolean;
    overRedaction: boolean;
  };

  const [errorState, setErrorState] = useState<ErrorState>({
    category: false,
    underRedaction: false,
    overRedaction: false,
  });

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

  const findRedactionTypesError = useCallback(
    (category: "underRedaction" | "overRedaction") => {
      if (getValues(category)) {
        return !Object.keys(getValues()).some(
          (key) => key.includes(`${category}-type-`) && getValues(key)
        );
      }
      return false;
    },
    [getValues]
  );

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
              isSubmitted && errorState.underRedaction
                ? {
                    children: "Select an under-redaction type",
                  }
                : undefined
            }
            fieldset={{
              legend: {
                children: "Types of redactions",
              },
            }}
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
              isSubmitted && errorState.overRedaction
                ? {
                    children: "Select an over-redaction type",
                  }
                : undefined
            }
            fieldset={{
              legend: {
                children: "Types of redactions",
              },
            }}
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
    const subscription = watch(
      (value: any, { name, type }: { name: any; type: any }) => {
        setErrorState((state) => ({
          ...state,
          category: !(
            getValues("underRedaction") || getValues("overRedaction")
          ),
          underRedaction: findRedactionTypesError("underRedaction"),
          overRedaction: findRedactionTypesError("overRedaction"),
        }));
        if (isSubmitted) {
          trigger();
        }
      }
    );
    return () => subscription.unsubscribe();
  }, [watch, isSubmitted, getValues, trigger, findRedactionTypesError]);

  return (
    <div
      className={classes.underOverRedactionContent}
      data-testid="under-over-redaction-content"
    >
      <section>
        <Checkboxes
          errorMessage={
            isSubmitted && errorState.category
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
          data-testid="checkboxes-under-over"
          name="redaction-category"
          items={redactionCategoryCheckboxItem}
          className="govuk-checkboxes--large"
        />
      </section>
    </div>
  );
};
