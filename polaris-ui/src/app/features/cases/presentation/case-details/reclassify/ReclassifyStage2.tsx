import {
  Select,
  Input,
  Radios,
  DateInput,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
type ReclassifyStage2Props = {
  presentationTitle: string;
};

export const ReclassifyStage2: React.FC<ReclassifyStage2Props> = ({
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext();

  if (!reclassifyContext) {
    return <div>Context is now available</div>;
  }
  const { state, dispatch } = reclassifyContext;

  const handleDocumentNameChange = (value: string | undefined) => {
    console.log("value>>>", value);
  };

  const handleDocumentUsedStatusChange = (value: string | undefined) => {};

  const getHeaderText = (
    type: "type1" | "type2" | "type3" | "type4" | "initial"
  ) => {
    switch (type) {
      case "type3":
        return "Enter the statement details";
      case "type4":
        return "Enter the exhibit details";
      default:
        return "Enter the document details";
    }
  };

  const getSubHeading = (
    type: "type1" | "type2" | "type3" | "type4" | "initial"
  ) => {
    switch (type) {
      case "type3":
        return (
          <p>
            You're entering statement details for{" "}
            <strong>{presentationTitle}</strong>
          </p>
        );
      case "type4":
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
  return (
    <div>
      <h1>{getHeaderText(state.reclassifyType)}</h1>
      {getSubHeading(state.reclassifyType)}
      {state.reclassifyType !== "type3" && (
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

      {state.reclassifyType === "type4" && (
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

      {state.reclassifyType === "type3" && (
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
      {state.reclassifyType !== "type1" && (
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
