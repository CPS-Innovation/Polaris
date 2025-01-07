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

enum RadioButtoUsedState {
  USED = "Used",
  UNUSED = "Unused",
}

type Action = { type: "USED" } | { type: "UNUSED" } | { type: string };

type State = {
  RadioButtons: "Used" | "Unused";
};

const reducer = (state: State, action: Action): State => {
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

const initialState: State = {
  RadioButtons: "Used",
};

const ChangeRadio: React.FC<RadioProps> = ({ handleClose }) => {
  const [state, dispatch] = useReducer(reducer, initialState);

  const handleRadioButtonsChange = (value: string): void => {
    dispatch({ type: value });
  };

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
        onChange={(arg: any) => {
          handleRadioButtonsChange(arg);
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
