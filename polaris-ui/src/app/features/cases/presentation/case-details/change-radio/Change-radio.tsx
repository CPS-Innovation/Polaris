import React, { useReducer } from "react";
import {
  Input,
  Radios,
  LinkButton,
} from "../../../../../common/presentation/components";
import "./Change-radio.scss";

type ChangeRadioProps = {
  handleClose: () => void;
};

const ChangeRadio: React.FC<ChangeRadioProps> = ({ handleClose }) => {
  const initialState = {
    RadioButtonsChange: "Used",
  };

  const reducer = (state: "Used" | "Unused", action: any) => {
    switch (action.type) {
      case "Used":
        return {
          RadioButtonsChange: "Used",
        };
      case "Unused":
        return {
          RadioButtonsChange: "Unused",
        };
      default:
        return state;
    }
  };

  const [state, dispatch] = useReducer<any>(reducer, initialState);

  const handleRadioButtonsChange = (value: string | undefined) => {
    console.log("value: ", value);
    dispatch({ type: value });
  };
  console.log("state.RadioButtonsChange", state?.RadioButtonsChange);
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
        value={state?.RadioButtonsChange}
        name="reclassify-change-radio=buttons"
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
