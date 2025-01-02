import React from "react";
import { Input, Radios, LinkButton } from "../../../../../common/presentation/components";
import './Change-radio.scss';

type ChangeRadioProps = {
    handleClose: () => void;
}

const ChangeRadio: React.FC<ChangeRadioProps> = ({
    handleClose
}) => {

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
                                Do you want to change the document name of{" "}
                                <strong className={'classes.highlight'}>
                                    {'presentationTitle'}
                                </strong>
                                ?
                            </span>
                        ),
                    },
                }}
                // className={
                //     formDataErrors.documentNewNameErrorText
                //         ? "govuk-form-group--error"
                //         : ""
                // }
                key={"reclassify-change-document-name"}
                onChange={(val) => alert(val)}
                value={'state.formData.documentRenameStatus'}
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
                                    // errorMessage={
                                    //     formDataErrors.documentNewNameErrorText
                                    //         ? {
                                    //             children: formDataErrors.documentNewNameErrorText,
                                    //         }
                                    //         : undefined
                                    // }
                                    name="reclassify-document-new-name"
                                    type="text"
                                    value={'state.formData.documentNewName'}
                                    onChange={() => alert('fdlf')}
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
                data-testid="reclassify-rename"
            />
        </div>
    )
}

export { ChangeRadio }