import { useState, useEffect, useMemo } from "react";
import {
  Select,
  Input,
  Radios,
  DateInput,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyVariant } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import classes from "./Reclassify.module.scss";

type ReclassifyStage2Props = {
  presentationTitle: string;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
};

export const ReclassifyStage2: React.FC<ReclassifyStage2Props> = ({
  presentationTitle,
  getExhibitProducers,
  getStatementWitnessDetails,
}) => {
  const [loading, setLoading] = useState(false);
  const reclassifyContext = useReClassifyContext();

  const { state, dispatch } = reclassifyContext!;

  useEffect(() => {
    const fetchDataOnMount = async () => {
      if (
        state.reclassifyVariant === "EXHIBIT" &&
        state.exhibitProducers.length
      )
        return;
      if (
        state.reclassifyVariant === "STATEMENT" &&
        state.statementWitness.length
      )
        return;
      setLoading(true);
      try {
        if (state.reclassifyVariant === "EXHIBIT") {
          const result = await getExhibitProducers();
          dispatch({
            type: "ADD_EXHIBIT_PRODUCERS",
            payload: { exhibitProducers: result },
          });
        }
        if (state.reclassifyVariant === "STATEMENT") {
          const result = await getStatementWitnessDetails();
          dispatch({
            type: "ADD_STATEMENT_WITNESSS",
            payload: { statementWitness: result },
          });
        }
      } catch (error) {
        console.error("Error fetching data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDataOnMount();
  }, [
    getExhibitProducers,
    getStatementWitnessDetails,
    state.reclassifyVariant,
    dispatch,
    state.exhibitProducers.length,
    state.statementWitness.length,
  ]);

  const statementWitnessValues = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Select a Witness",
      disabled: true,
    };
    const mappedValues = state.statementWitness.map(
      ({ witness: { id, name } }) => ({
        value: id,
        children: name,
      })
    );
    return [defaultValue, ...mappedValues];
  }, [state.statementWitness]);

  const exhibitProducersValues = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Select a Producer",
      disabled: true,
    };
    const mappedValues = state.exhibitProducers.map(({ id, fullName }) => ({
      value: id,
      children: fullName,
    }));
    const otherOption = {
      value: "other",
      children: "Other producer or witness",
      disabled: false,
    };
    return [defaultValue, ...mappedValues, otherOption];
  }, [state.exhibitProducers]);

  const getHeaderText = (varaint: ReclassifyVariant) => {
    switch (varaint) {
      case "STATEMENT":
        return "Enter the statement details";
      case "EXHIBIT":
        return "Enter the exhibit details";
      default:
        return "Enter the document details";
    }
  };

  const getSubHeading = (type: ReclassifyVariant) => {
    switch (type) {
      case "STATEMENT":
        return (
          <p>
            You're entering statement details for{" "}
            <strong>{presentationTitle}</strong>
          </p>
        );
      case "EXHIBIT":
        return (
          <p>
            You're entering exhibit details for{" "}
            <strong>{presentationTitle}</strong>
          </p>
        );

      default:
        return;
    }
  };

  const handleDocumentRenameStatusChange = (value: string | undefined) => {
    if (value)
      dispatch({
        type: "UPDATE_DOCUMENT_RENAME_STATUS",
        payload: { value: value as "YES" | "NO" },
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

  const handleUpdateExhibitItem = (value: string) => {
    dispatch({
      type: "UPDATE_EXHIBIT_ITEM",
      payload: { value: value },
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

  const handleUpdateStatementWitnessId = (value: string) => {
    dispatch({
      type: "UPDATE_STATEMENT_WITNESS_ID",
      payload: { value: value },
    });
  };

  const handleStatementDateChange = (event: any) => {
    let type: "day" | "month" | "year" = "day";
    if (event.target.name === "statement-date-day") {
      type = "day";
    }
    if (event.target.name === "statement-date-month") {
      type = "month";
    }
    if (event.target.name === "statement-date-year") {
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
  if (loading) {
    return <div>loading data</div>;
  }
  return (
    <div>
      <h1>{getHeaderText(state.reclassifyVariant)}</h1>
      {getSubHeading(state.reclassifyVariant)}
      {state.reclassifyVariant !== "STATEMENT" && (
        <Radios
          hint={{
            children: (
              <span>
                Do you want to change the document name of{" "}
                <strong className="docType">{presentationTitle}</strong>?
              </span>
            ),
          }}
          key={"change-document-name"}
          onChange={handleDocumentRenameStatusChange}
          value={state.formData.documentRenameStatus}
          name="radio-change-document-name"
          items={[
            {
              children: "Yes",
              conditional: {
                children: [
                  <Input
                    id="document-new-name"
                    className="govuk-input--width-10"
                    label={{
                      children: "Enter new document name",
                    }}
                    name="document-new-name"
                    type="text"
                    value={state.formData.documentNewName}
                    onChange={handleDocumentNewName}
                  />,
                ],
              },
              value: "YES",
            },
            {
              children: "No",
              value: "NO",
            },
          ]}
        />
      )}

      {state.reclassifyVariant === "EXHIBIT" && (
        <div>
          <Input
            id="exhibit-item"
            className="govuk-input--width-10"
            label={{
              children: "Exhibit Item",
            }}
            name="exhibit-item"
            type="text"
            value={state.formData.exhibitItem}
            onChange={handleUpdateExhibitItem}
          />
          <Input
            id="exhibit-reference"
            className="govuk-input--width-10"
            label={{
              children: "Exhibit Reference",
            }}
            name="exhibit-reference"
            type="text"
            value={state.formData.exhibitReference}
            onChange={handleUpdateExhibitReference}
          />
          <Input
            id="exhibit-item-name"
            className="govuk-input--width-10"
            label={{
              children: "Item Name",
            }}
            name="exhibit-item-name"
            type="text"
            value={state.formData.exhibitItemName}
            onChange={handleUpdateExhibitItemName}
          />

          <div className={classes.producerSelectWrapper}>
            <Select
              id="exhibit-select-producer"
              items={exhibitProducersValues}
              label={{
                children: "Select existing producer or witness",
              }}
              name="exhibit-select-producer"
              value={state.formData.exhibitProducerId}
              onChange={(ev) => handleUpdateExhibitProducerId(ev.target.value)}
            />

            {state.formData.exhibitProducerId === "other" && (
              <div className={classes.otherProducerWrapper}>
                <Input
                  id="exhibit-other-producer-name"
                  className={`govuk-input--width-10 ${classes.otherProducerName}`}
                  label={{
                    children: "Enter name",
                  }}
                  name="exhibit-other-producer-name"
                  type="text"
                  value={state.formData.exhibitOtherProducerValue}
                  onChange={handleUpdateOtherProducerName}
                />
              </div>
            )}
          </div>
        </div>
      )}

      {state.reclassifyVariant === "STATEMENT" && (
        <div>
          <Select
            id="statement-select-witness"
            items={statementWitnessValues}
            label={{
              children: "Select witness",
            }}
            name="statement-select-witness"
            value={state.formData.statementWitnessId}
            onChange={(ev) => handleUpdateStatementWitnessId(ev.target.value)}
          />
          <DateInput
            fieldset={{
              legend: {
                children: <span>Statement date</span>,
              },
            }}
            hint={{
              children: (
                <span>
                  For example, 27 3 2024 <br /> Leave blank if the document is
                  Undated.
                </span>
              ),
            }}
            id="statement-date"
            items={[
              {
                className: "govuk-input--width-2",
                name: "day",
                value: state.formData.statementDay,
              },
              {
                className: "govuk-input--width-2",
                name: "month",
                value: state.formData.statementMonth,
              },
              {
                className: "govuk-input--width-4",
                name: "year",
                value: state.formData.statementYear,
              },
            ]}
            namePrefix="statement-date"
            onChange={handleStatementDateChange}
          />

          <Input
            id="statement-number"
            className="govuk-input--width-10"
            label={{
              children: "Statement Number",
            }}
            hint={{
              children: "Already in use #6, #5, #4, #3, #2 and #1",
            }}
            name="statement-number"
            type="number"
            value={state.formData.statementNumber}
            onChange={handleUpdateStatementNumber}
          />
        </div>
      )}
      {state.reclassifyVariant !== "IMMEDIATE" && (
        <Radios
          hint={{
            children: <span>What is the document status?</span>,
          }}
          key={"document-used-status"}
          onChange={handleDocumentUsedStatusChange}
          value={state.formData.documentUsedStatus}
          name="radio-document-used-status"
          items={[
            {
              children: "Used",

              value: "YES",
            },
            {
              children: "Unused",
              value: "NO",
            },
          ]}
        />
      )}
    </div>
  );
};
