import {
  BackLink,
  Tooltip,
  LinkButton,
  Checkboxes,
  Select,
  Button,
  Input,
  Radios,
  DateInput,
} from "../../../../../common/presentation/components";

type ReclassifyPage2Props = {};

export const ReclassifyPage2 = () => {
  const handleDocumentNameChange = (value: string | undefined) => {
    console.log("value>>>", value);
  };

  const handleDocumentUsedStatusChange = (value: string | undefined) => {};

  const handleStatementDateChange = (value: any) => {};
  return (
    <div>
      <Radios
        fieldset={{
          legend: {
            children: "Select the document details",
            className: "govuk-fieldset__legend--l",
            isPageHeading: true,
          },
        }}
        hint={{
          children: (
            <span>
              Do you want to change the document name of{" "}
              <strong className="docType">Asset Rec 1</strong>?
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
        id="select-witness"
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
        name="select-witness"
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
      <div>
        <Button>Continue</Button>
        <LinkButton onClick={() => {}}>Cancel</LinkButton>
      </div>
    </div>
  );
};
