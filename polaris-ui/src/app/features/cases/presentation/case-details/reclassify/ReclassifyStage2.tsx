import { useState, useEffect } from "react";
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

  const handleDocumentNameChange = (value: string | undefined) => {
    console.log("value>>>", value);
  };

  const handleDocumentUsedStatusChange = (value: string | undefined) => {};

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

  const handleStatementDateChange = (value: any) => {};
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
          onChange={handleDocumentNameChange}
          value={"Yes"}
          name="radio-change-document-name"
          items={[
            {
              children: "Yes",
              conditional: {
                children: [
                  <label
                    key="0"
                    className="govuk-label"
                    htmlFor="document-new-name"
                  >
                    Enter new document name
                  </label>,
                  <input
                    key="1"
                    className="govuk-input  govuk-input--width-10"
                    id="document-new-name"
                    name="document-new-name"
                    type="text"
                  />,
                ],
              },
              value: "Yes",
            },
            {
              children: "No",
              value: "No",
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
          />
          <Input
            id="exhibit-reference"
            className="govuk-input--width-10"
            label={{
              children: "Exhibit Reference",
            }}
            name="exhibit-reference"
            type="text"
          />
          <Input
            id="exhibit-item-name"
            className="govuk-input--width-10"
            label={{
              children: "Item Name",
            }}
            name="exhibit-item-name"
            type="text"
          />

          <Select
            id="exhibit-select-witness"
            items={[
              {
                children: "Witness 1",
                value: 1,
              },
              {
                children: "Witness 2",
                value: 2,
              },
              {
                children: "Select a Witness",
                disabled: true,
                value: 3,
              },
            ]}
            label={{
              children: "Select witness",
            }}
            name="exhibit-select-witness"
            value={3}
          />
        </div>
      )}

      {state.reclassifyVariant === "STATEMENT" && (
        <div>
          <Select
            id="statement-select-witness"
            items={[
              {
                children: "Witness 1",
                value: 1,
              },
              {
                children: "Witness 2",
                value: 2,
              },
              {
                children: "Select a Witness",
                disabled: true,
                value: 3,
              },
            ]}
            label={{
              children: "Select witness",
            }}
            name="statement-select-witness"
            value={3}
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
              },
              {
                className: "govuk-input--width-2",
                name: "month",
              },
              {
                className: "govuk-input--width-4",
                name: "year",
              },
            ]}
            namePrefix="dob"
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
            type="text"
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
          value={"Unused"}
          name="radio-document-used-status"
          items={[
            {
              children: "Used",

              value: "Used",
            },
            {
              children: "Unused",
              value: "Unused",
            },
          ]}
        />
      )}
    </div>
  );
};
