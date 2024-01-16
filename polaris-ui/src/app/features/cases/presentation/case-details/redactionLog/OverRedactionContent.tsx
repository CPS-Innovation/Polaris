import { useState, useEffect, useCallback } from "react";
import classes from "./OverRedactionContent.module.scss";
import {
  Checkboxes,
  CheckboxesProps,
} from "../../../../../common/presentation/components/Checkboxes";
import { Radios } from "../../../../../common/presentation/components/Radios";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";

type OverRedactionContentProps = {
  redactionTypes: RedactionTypeData[];
  savedRedactionTypes?: RedactionTypeData[];
  showErrors?: boolean;
  register: any;
  setValue: any;
  getValues: any;
  errors: any;
  watch: any;
  trigger: any;
  isSubmitted: boolean;
};

export const OverRedactionContent: React.FC<OverRedactionContentProps> = ({
  redactionTypes,
  savedRedactionTypes,
  register,
  setValue,
  getValues,
  errors,
  watch,
  trigger,
  isSubmitted,
}) => {
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
    return redactionTypes.map(
      (type) =>
        ({
          checked: getValues(`${category}-type-${type.id}`),
          children: type.name,
          id: `checkbox-${category}-type-${type.id}`,
          value: type.id,
          ...register(`${category}-type-${type.id}`, {
            validate: () => errorState[category] !== true,
          }),
        } as CheckboxesProps["items"])
    );
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
            name="redaction types"
            items={redactionTypeCheckboxItems("underRedaction")}
            className={`govuk-checkboxes--small ${classes.redactionTypes} `}
          />,
        ],
      },
    } as unknown as CheckboxesProps["items"],
    {
      checked: getValues(`overRedaction`),
      children: "Over Redaction",
      id: "checkbox-over-redaction",
      ...register(`overRedaction`, {
        validate: () => errorState.category !== true,
      }),
      conditional: {
        children: [
          <div>
            <Radios
              value={getValues(`returnToIA`)}
              name="return to investigative agency"
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
            />
            <Checkboxes
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
              name="redaction types"
              items={redactionTypeCheckboxItems("overRedaction")}
              className={`govuk-checkboxes--small ${classes.redactionTypes} `}
            />
          </div>,
        ],
      },
    } as unknown as CheckboxesProps["items"],
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
    <div className={classes.overRedactionContent}>
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
          name="redactionCategory"
          items={redactionCategoryCheckboxItem}
          className="govuk-checkboxes--large"
        />
      </section>
    </div>
  );
};
