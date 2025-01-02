import React, { useReducer } from "react";
import { Input, Radios, LinkButton } from "../../../../../common/presentation/components";
import './Change-radio.scss';

type ChangeRadioProps = {
    handleClose: () => void;
}

const ChangeRadio: React.FC<ChangeRadioProps> = ({
    handleClose
}) => {

    const initialState = {
        RadioButtonsChange: 'used'
    }

    const reducer = (state: "used" | "unused", action: any) => {
        switch (action.type) {
            case 'USED':
                return {
                    RadioButtonsChange: 'used'
                }
            case 'UNUSED':
                return {
                    RadioButtonsChange: 'unused'
                }
            default:
                return state
        };
    };

    const [state, dispatch] = useReducer(reducer, initialState)

    const handleRadioButtonsChange = (value: boolean) => {
        dispatch({ type: value.toString().toUpperCase(), payload: null })
    }

    return (
        <div>
            <LinkButton
                className={"govuk-back-link"}
                onClick={handleClose}
            >
                Back
            </LinkButton>
            <Radios
                fieldset={{
                    legend: {
                        children: (
                            <span>
                                What is the document status?
                            </span>
                        ),
                    },
                }}
                // className={
                //     formDataErrors.documentNewNameErrorText
                //         ? "govuk-form-group--error"
                //         : ""
                // }
                key={"reclassify-change-radio=buttons"}
                onChange={(arg: any) => {
                    arg &&
                        handleRadioButtonsChange(arg);
                }
                }
                value={state.used}
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
            {console.log("end: ", state.RadioButtonsChange)}
        </div>
    )
}

export { ChangeRadio }