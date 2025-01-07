import React, { useReducer } from "react";
import {
  Input,
  Radios,
  LinkButton,
} from "../../../../../common/presentation/components";
import "./Change-radio.scss";

type RadioProps = {
  handleClose: () => void;
};

type DocumentState = {
  RadioButtons: "Used" | "Unused";
};

enum RadioButtoUsedState {
  USED = "Used",
  UNUSED = "Unused",
}

type RadioAction = { type: "Used" } | { type: "Unused" } | { type: string };

const ChangeRadio: React.FC<RadioProps> = ({ handleClose }) => {
  const initialState = {
    RadioButtons: RadioButtoUsedState.UNUSED,
  };

  const reducer = (state: DocumentState, action: RadioAction) => {
    switch (action.type) {
      case RadioButtoUsedState.USED:
        return {
          RadioButtons: "Used",
        };
      case RadioButtoUsedState.UNUSED:
        return {
          RadioButtons: "Unused",
        };
      default:
        return state;
    }
  };

  const [state, dispatch] = useReducer(reducer, initialState);

  const handleRadioButtonsChange = (value: string): any => {
    dispatch({ type: "Unused" });
  };

  console.log("state.RadioButtonsChange", state.RadioButtons);

  return (
    <div>
      <LinkButton className={"govuk-back-link"} onClick={handleClose}>
        Back
      </LinkButton>
      <Radios
        fieldset={{
          legend: {
            children: <span>What is the document status?</span>,
          },
        }}
        // className={
        //     formDataErrors.documentNewNameErrorText
        //         ? "govuk-form-group--error"
        //         : ""
        // }
        key={"reclassify-change-radio-buttons"}
        onChange={(arg: string | undefined) => {
          arg && handleRadioButtonsChange(arg);
        }}
        value={state.RadioButtons}
        name="reclassify-change-radio-buttons"
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
        data-testid="reclassify-change"
      />
    </div>
  );
};

export { ChangeRadio };
