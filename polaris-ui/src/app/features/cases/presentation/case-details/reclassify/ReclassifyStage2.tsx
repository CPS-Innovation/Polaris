import { useState, useEffect, useMemo, useRef, useCallback } from "react";
import {
  Select,
  Input,
  Radios,
  DateInput,
  ErrorSummary,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyVariant } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { StatementWitnessNumber } from "./data/StatementWitnessNumber";
import { statementNumberText } from "./utils/statementNumberText";
import { FormDataErrors } from "./data/FormDataErrors";
import classes from "./Reclassify.module.scss";

type ReclassifyStage2Props = {
  presentationTitle: string;
  formDataErrors: FormDataErrors;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  getWitnessStatementNumbers: (
    witnessId: number
  ) => Promise<StatementWitnessNumber[]>;
  handleBackBtnClick: () => void;
  handleLookUpDataError: (errorMessage: string) => void;
  handleCheckContentLoaded: (value: boolean) => void;
};

export const ReclassifyStage2: React.FC<ReclassifyStage2Props> = ({
  presentationTitle,
  formDataErrors,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  handleBackBtnClick,
  handleLookUpDataError,
  handleCheckContentLoaded,
}) => {
  const [loading, setLoading] = useState(false);
  const reclassifyContext = useReClassifyContext();

  const { state, dispatch } = reclassifyContext!;
  const errorSummaryRef = useRef(null);
  const backButtonRef = useRef(null);
  useEffect(() => {
    const fetchDataOnMount = async () => {
      if (
        state.reclassifyVariant === "Exhibit" &&
        state.exhibitProducers !== null
      ) {
        return;
      }
      if (
        state.reclassifyVariant === "Statement" &&
        state.statementWitness !== null
      ) {
        return;
      }
      try {
        if (state.reclassifyVariant === "Exhibit") {
          setLoading(true);
          const result = await getExhibitProducers();
          dispatch({
            type: "ADD_EXHIBIT_PRODUCERS",
            payload: { exhibitProducers: result },
          });
        }
        if (state.reclassifyVariant === "Statement") {
          setLoading(true);
          const result = await getStatementWitnessDetails();
          dispatch({
            type: "ADD_STATEMENT_WITNESSS",
            payload: { statementWitness: result },
          });
        }
      } catch (error) {
        console.error("Error fetching data:", error);
        if (state.reclassifyVariant === "Exhibit")
          handleLookUpDataError("Failed to retrieve exhibit producer data");
        if (state.reclassifyVariant === "Statement")
          handleLookUpDataError("Failed to retrieve statement witness details");
      } finally {
        setLoading(false);
      }
    };

    fetchDataOnMount();
  }, [state.reclassifyVariant]);

  useEffect(() => {
    if (!loading && backButtonRef.current)
      (backButtonRef.current as HTMLButtonElement).focus();
  }, [loading]);

  useEffect(() => {
    handleCheckContentLoaded(loading);
  }, [loading]);

  const statementWitnessValues = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Select a witness",
      disabled: true,
    };
    if (!state.statementWitness) {
      return [defaultValue];
    }
    const mappedValues = state.statementWitness.map(({ id, name }) => ({
      value: id,
      children: name,
    }));
    return [defaultValue, ...mappedValues];
  }, [state.statementWitness]);

  const exhibitProducersValues = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Select a Producer",
      disabled: true,
    };
    const otherOption = {
      value: "other",
      children: "Other producer or witness",
      disabled: false,
    };
    if (!state.exhibitProducers) {
      return [defaultValue, otherOption];
    }
    const mappedValues = state.exhibitProducers.map(
      ({ id, exhibitProducer }) => ({
        value: id,
        children: exhibitProducer,
      })
    );

    return [defaultValue, ...mappedValues, otherOption];
  }, [state.exhibitProducers]);

  const getSubHeading = (type: ReclassifyVariant) => {
    switch (type) {
      case "Statement":
        return (
          <p>
            You're entering statement details for{" "}
            <strong className={classes.highlight}>{presentationTitle}</strong>
          </p>
        );
      case "Exhibit":
        return (
          <p>
            You're entering exhibit details for{" "}
            <strong className={classes.highlight}>{presentationTitle}</strong>
          </p>
        );

      default:
        return;
    }
  };

  const handleDocumentRenameStatusChange = (value: string | undefined) => {
    if (!value) {
      return;
    }
    dispatch({
      type: "UPDATE_DOCUMENT_RENAME_STATUS",
      payload: { value: value as "YES" | "NO" },
    });
    if (value === "YES") {
      dispatch({
        type: "UPDATE_DOCUMENT_NEW_NAME",
        payload: { newName: presentationTitle },
      });
      return;
    }
    dispatch({
      type: "UPDATE_DOCUMENT_NEW_NAME",
      payload: { newName: "" },
    });
  };

  const handleDocumentNewName = (value: string) => {
    dispatch({
      type: "UPDATE_DOCUMENT_NEW_NAME",
      payload: { newName: value },
    });
  };

  const handleDocumentUsedStatusChange = (value: string | undefined) => {
    if (value)
      dispatch({
        type: "UPDATE_DOCUMENT_USED_STATUS",
        payload: { value: value as "YES" | "NO" },
      });
  };

  const handleUpdateExhibitReference = (value: string) => {
    dispatch({
      type: "UPDATE_EXHIBIT_ITEM_REFERENCE",
      payload: { value: value },
    });
  };

  const handleUpdateExhibitItemName = (value: string) => {
    dispatch({
      type: "UPDATE_EXHIBIT_ITEM_NAME",
      payload: { value: value },
    });
  };

  const handleUpdateExhibitProducerId = (value: string) => {
    dispatch({
      type: "UPDATE_EXHIBIT_PRODUCER_ID",
      payload: { value: value },
    });
    dispatch({
      type: "UPDATE_EXHIBIT_OTHER_PRODUCER_VALUE",
      payload: { value: "" },
    });
  };

  const handleUpdateOtherProducerName = (value: string) => {
    dispatch({
      type: "UPDATE_EXHIBIT_OTHER_PRODUCER_VALUE",
      payload: { value: value },
    });
  };

  const handleUpdateStatementWitnessId = async (value: string) => {
    dispatch({
      type: "UPDATE_STATEMENT_WITNESS_ID",
      payload: { value: value },
    });
    dispatch({
      type: "UPDATE_STATEMENT_WITNESS_NUMBERS",
      payload: { witnessId: +value, statementNumbers: [] },
    });
    try {
      const data = await getWitnessStatementNumbers(+value);
      const numbers = (
        data.filter((item) => item.statementNumber !== null) as unknown as {
          witnessId: number;
          statementNumber: number;
        }[]
      ).map((item) => item.statementNumber);
      dispatch({
        type: "UPDATE_STATEMENT_WITNESS_NUMBERS",
        payload: { witnessId: +value, statementNumbers: numbers },
      });
    } catch (e) {
      handleLookUpDataError("Failed to retrieve statement witness numbers");
    }
  };

  const handleStatementDateChange = (event: any) => {
    let type: "day" | "month" | "year" = "day";

    if (event.target.name === "reclassify-statement-date-month") {
      type = "month";
    }
    if (event.target.name === "reclassify-statement-date-year") {
      type = "year";
    }
    if (type)
      dispatch({
        type: "UPDATE_STATEMENT_DATE",
        payload: { type: type, value: event.target.value },
      });
  };

  const handleUpdateStatementNumber = (value: string) => {
    dispatch({
      type: "UPDATE_STATEMENT_NUMBER",
      payload: { value: value },
    });
  };

  const getDateInputLink = useCallback(() => {
    if (formDataErrors.statementDayErrorText) {
      return "#reclassify-statement-day";
    }
    if (formDataErrors.statementMonthErrorText) {
      return "#reclassify-statement-month";
    }

    return "#reclassify-statement-year";
  }, [
    formDataErrors.statementDayErrorText,
    formDataErrors.statementMonthErrorText,
  ]);

  const errorSummaryProperties = useCallback(
    (inputName: keyof FormDataErrors) => {
      switch (inputName) {
        case "documentNewNameErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-document-new-name",
            "data-testid": "reclassify-document-new-name-link",
          };
        case "exhibitItemNameErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-exhibit-item-name",
            "data-testid": "reclassify-exhibit-item-name-link",
          };
        case "otherExhibitProducerErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-exhibit-other-producer-name",
            "data-testid": "reclassify-exhibit-other-producer-name-link",
          };
        case "exhibitReferenceErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-exhibit-reference",
            "data-testid": "reclassify-exhibit-reference-link",
          };
        case "statementWitnessErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-statement-witness",
            "data-testid": "reclassify-statement-witness-link",
          };
        case "statementNumberErrorText":
          return {
            children: formDataErrors[inputName],
            href: "#reclassify-statement-number",
            "data-testid": "reclassify-statement-number-link",
          };
        case "statementDateErrorText":
          return {
            children: formDataErrors.statementDateErrorText,
            href: getDateInputLink(),
            "data-testid": "reclassify-statement-date-link",
          };
      }
    },
    [formDataErrors, getDateInputLink]
  );

  const errorSummaryList = useMemo(() => {
    const validErrors = Object.fromEntries(
      Object.entries(formDataErrors).filter(
        ([key, value]) =>
          value !== "" &&
          key !== "statementDayErrorText" &&
          key !== "statementMonthErrorText" &&
          key !== "statementYearErrorText"
      )
    );
    const errorSummary = Object.keys(validErrors).map((error, index) => ({
      reactListKey: `${index}`,
      ...errorSummaryProperties(error as keyof FormDataErrors)!,
    }));
    return errorSummary;
  }, [formDataErrors, errorSummaryProperties]);

  useEffect(() => {
    if (errorSummaryList.length && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [errorSummaryList]);

  if (loading) {
    return <div>loading data...</div>;
  }

  if (
    state.reclassifyVariant === "Statement" &&
    !state.statementWitness?.length
  ) {
    return (
      <>
        <h1>There is a problem</h1>
        <p>
          Cannot continue with reclassification as the statement does not have
          any witness
        </p>
      </>
    );
  }
  return (
    <div role="main" aria-labelledby="main-description">
      <div className="govuk-checkboxes__conditional">
        {!!errorSummaryList.length && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={classes.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"reclassify-error-summary"}
              className={classes.errorSummary}
              errorList={errorSummaryList}
            />
          </div>
        )}
        {getSubHeading(state.reclassifyVariant)}

        {state.reclassifyVariant !== "Statement" &&
          state.reclassifyVariant !== "Exhibit" && (
            <Radios
              fieldset={{
                legend: {
                  children: (
                    <span>
                      Do you want to change the document name of{" "}
                      <strong className={classes.highlight}>
                        {presentationTitle}
                      </strong>
                      ?
                    </span>
                  ),
                },
              }}
              className={
                formDataErrors.documentNewNameErrorText
                  ? "govuk-form-group--error"
                  : ""
              }
              key={"reclassify-change-document-name"}
              onChange={handleDocumentRenameStatusChange}
              value={state.formData.documentRenameStatus}
              name="reclassify-change-document-name"
              items={[
                {
                  children: "Yes",
                  conditional: {
                    children: [
                      <Input
                        key="reclassify-document-new-name"
                        id="reclassify-document-new-name"
                        data-testid="reclassify-document-new-name"
                        className="govuk-input--width-20"
                        label={{
                          children: "Enter new document name",
                        }}
                        errorMessage={
                          formDataErrors.documentNewNameErrorText
                            ? {
                                children:
                                  formDataErrors.documentNewNameErrorText,
                              }
                            : undefined
                        }
                        name="reclassify-document-new-name"
                        type="text"
                        value={state.formData.documentNewName}
                        onChange={handleDocumentNewName}
                        disabled={
                          state.reClassifySaveStatus === "saving" ||
                          state.reClassifySaveStatus === "success"
                        }
                      />,
                    ],
                  },
                  value: "YES",
                  disabled:
                    state.reClassifySaveStatus === "saving" ||
                    state.reClassifySaveStatus === "success",
                },
                {
                  children: "No",
                  value: "NO",
                  disabled:
                    state.reClassifySaveStatus === "saving" ||
                    state.reClassifySaveStatus === "success",
                },
              ]}
              data-testid="reclassify-rename"
            />
          )}

        {state.reclassifyVariant === "Exhibit" && (
          <div>
            <Input
              id="reclassify-exhibit-item-name"
              data-testid="reclassify-exhibit-item-name"
              className="govuk-input--width-20"
              errorMessage={
                formDataErrors.exhibitItemNameErrorText
                  ? {
                      children: formDataErrors.exhibitItemNameErrorText,
                    }
                  : undefined
              }
              label={{
                children: "Item Name",
              }}
              name="reclassify-exhibit-item-name"
              type="text"
              value={state.formData.exhibitItemName}
              onChange={handleUpdateExhibitItemName}
              disabled={
                state.reClassifySaveStatus === "saving" ||
                state.reClassifySaveStatus === "success"
              }
            />
            <Input
              id="reclassify-exhibit-reference"
              data-testid="reclassify-exhibit-reference"
              errorMessage={
                formDataErrors.exhibitReferenceErrorText
                  ? {
                      children: formDataErrors.exhibitReferenceErrorText,
                    }
                  : undefined
              }
              className="govuk-input--width-20"
              label={{
                children: "Exhibit Reference",
              }}
              name="reclassify-exhibit-reference"
              type="text"
              value={state.formData.exhibitReference}
              onChange={handleUpdateExhibitReference}
              disabled={
                state.reClassifySaveStatus === "saving" ||
                state.reClassifySaveStatus === "success"
              }
            />

            <div className={classes.producerSelectWrapper}>
              <Select
                id="reclassify-exhibit-producer"
                data-testid="reclassify-exhibit-producer"
                items={exhibitProducersValues}
                label={{
                  children: "Select existing producer or witness",
                }}
                errorMessage={
                  formDataErrors.otherExhibitProducerErrorText
                    ? {
                        children: formDataErrors.otherExhibitProducerErrorText,
                      }
                    : undefined
                }
                name="reclassify-exhibit-producer"
                value={state.formData.exhibitProducerId}
                onChange={(ev) =>
                  handleUpdateExhibitProducerId(ev.target.value)
                }
                disabled={
                  state.reClassifySaveStatus === "saving" ||
                  state.reClassifySaveStatus === "success"
                }
              />

              {state.formData.exhibitProducerId === "other" && (
                <div
                  className={`${
                    formDataErrors.otherExhibitProducerErrorText
                      ? classes.otherProducerNameError
                      : classes.otherProducerWrapper
                  }`}
                >
                  <Input
                    id="reclassify-exhibit-other-producer-name"
                    data-testid="reclassify-exhibit-other-producer-name"
                    className="govuk-input--width-20"
                    label={{
                      children: "Enter name",
                    }}
                    aria-label="Enter other producer or witness name"
                    errorMessage={
                      formDataErrors.otherExhibitProducerErrorText
                        ? {
                            children:
                              formDataErrors.otherExhibitProducerErrorText,
                          }
                        : undefined
                    }
                    name="reclassify-exhibit-other-producer-name"
                    type="text"
                    value={state.formData.exhibitOtherProducerValue}
                    onChange={handleUpdateOtherProducerName}
                    disabled={
                      state.reClassifySaveStatus === "saving" ||
                      state.reClassifySaveStatus === "success"
                    }
                  />
                </div>
              )}
            </div>
          </div>
        )}

        {state.reclassifyVariant === "Statement" && (
          <div>
            <Select
              id="reclassify-statement-witness"
              data-testid="reclassify-statement-witness"
              errorMessage={
                formDataErrors.statementWitnessErrorText
                  ? {
                      children: formDataErrors.statementWitnessErrorText,
                    }
                  : undefined
              }
              items={statementWitnessValues}
              label={{
                children: "Select witness",
              }}
              name="reclassify-statement-witness"
              value={state.formData.statementWitnessId}
              onChange={(ev) => handleUpdateStatementWitnessId(ev.target.value)}
              disabled={
                state.reClassifySaveStatus === "saving" ||
                state.reClassifySaveStatus === "success"
              }
            />
            <DateInput
              errorMessage={
                formDataErrors.statementDateErrorText
                  ? {
                      children: formDataErrors.statementDateErrorText,
                    }
                  : undefined
              }
              fieldset={{
                legend: {
                  children: <span>Statement date</span>,
                },
              }}
              hint={{
                children: <span>For example, 27 3 2024</span>,
              }}
              id="reclassify-statement-date"
              items={[
                {
                  id: "reclassify-statement-day",
                  className: `govuk-input--width-2 ${
                    formDataErrors.statementDayErrorText
                      ? "govuk-input--error"
                      : ""
                  }`,
                  name: "day",
                  value: state.formData.statementDay,
                  disabled:
                    state.reClassifySaveStatus === "saving" ||
                    state.reClassifySaveStatus === "success",
                },
                {
                  id: "reclassify-statement-month",
                  className: `govuk-input--width-2 ${
                    formDataErrors.statementMonthErrorText
                      ? "govuk-input--error"
                      : ""
                  }`,
                  name: "month",
                  value: state.formData.statementMonth,
                  disabled:
                    state.reClassifySaveStatus === "saving" ||
                    state.reClassifySaveStatus === "success",
                },
                {
                  id: "reclassify-statement-year",
                  className: `govuk-input--width-4 ${
                    formDataErrors.statementYearErrorText
                      ? "govuk-input--error"
                      : ""
                  }`,
                  name: "year",
                  value: state.formData.statementYear,
                  disabled:
                    state.reClassifySaveStatus === "saving" ||
                    state.reClassifySaveStatus === "success",
                },
              ]}
              namePrefix="reclassify-statement-date"
              onChange={handleStatementDateChange}
            />

            <Input
              id="reclassify-statement-number"
              data-testid="reclassify-statement-number"
              errorMessage={
                formDataErrors.statementNumberErrorText
                  ? {
                      children: formDataErrors.statementNumberErrorText,
                    }
                  : undefined
              }
              className="govuk-input--width-10"
              label={{
                children: "Statement Number",
              }}
              hint={{
                children: statementNumberText(
                  state.statementWitnessNumbers[
                    state.formData.statementWitnessId
                  ] ?? []
                ),
              }}
              name="reclassify-statement-number"
              type="number"
              value={state.formData.statementNumber}
              onChange={handleUpdateStatementNumber}
              disabled={
                state.reClassifySaveStatus === "saving" ||
                state.reClassifySaveStatus === "success"
              }
            />
          </div>
        )}
        {state.reclassifyVariant !== "Immediate" && (
          <Radios
            fieldset={{
              legend: {
                children: <span>What is the document status?</span>,
              },
            }}
            key={"document-used-status"}
            onChange={handleDocumentUsedStatusChange}
            value={state.formData.documentUsedStatus}
            name="radio-document-used-status"
            items={[
              {
                children: "Used",

                value: "YES",
                disabled:
                  state.reClassifySaveStatus === "saving" ||
                  state.reClassifySaveStatus === "success",
              },
              {
                children: "Unused",
                value: "NO",
                disabled:
                  state.reClassifySaveStatus === "saving" ||
                  state.reClassifySaveStatus === "success",
              },
            ]}
          />
        )}
      </div>
      <p>&nbsp;</p>
    </div>
  );
};
