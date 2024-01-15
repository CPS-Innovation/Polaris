import { useMemo, useState, useEffect } from "react";
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
  showErrors?: boolean;
  register: any;
  setValue: any;
  getValues: any;
  errors: any;
  watch: any;
  trigger: any;
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

  const returnToIACheckboxItem = [
    {
      checked: false,
      children: "Return to Investigative Agency",
      id: "1",
      value: "1",
    } as CheckboxesProps["items"],
  ];

  const findRedactionTypesError = (
    category: "underRedaction" | "overRedaction"
  ) => {
    if (getValues(category)) {
      return !Object.keys(getValues()).some(
        (key) => key.includes(`${category}-type-`) && getValues(key)
      );
    }
    return false;
  };

  const redactionCategoryCheckboxItem = [
    {
      checked: getValues(`underRedaction`),
      children: "Under Redaction",
      id: "checkbox-under-redaction",
      ...register(`underRedaction`, {
        validate: () => errorState.category !== true,
      }),
      conditional: {
        children: [
          <Checkboxes
            errorMessage={
              errorState.underRedaction
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
          // <Checkboxes
          //   data-testid="checkboxes-under-return-to-IA"
          //   name="Return to Investigative Agency"
          //   items={returnToIACheckboxItem}
          //   className="govuk-checkboxes--large"
          //   // onChange={handleUnderOverSelection}
          // />,
          <Checkboxes
            errorMessage={
              errorState.overRedaction
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
          />,
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
        trigger();
      }
    );
    return () => subscription.unsubscribe();
  }, [watch]);

  return (
    <div className={classes.underRedactionContent}>
      <div className={classes.selectInputWrapper}>
        <section className={classes.underRedactionSection}>
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
            data-testid="checkboxes-under-over"
            name="redactionCategory"
            items={redactionCategoryCheckboxItem}
            className="govuk-checkboxes--large"
          />
        </section>
      </div>
    </div>
  );
};
