import { useState } from "react";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import {
  Select,
  Input,
  TextArea,
  Button,
  EditButton,
  LinkButton,
  Guidance,
} from "../../../../../common/presentation/components";
import { ReactComponent as EditIcon } from "../../../../../common/presentation/svgs/edit.svg";
import { UnderRedactionContent } from "./UnderRedactionContent";
import classes from "./RedactionLogContent.module.scss";
import { useForm, Controller } from "react-hook-form";
import { SavingStatus } from "../../../domain/gateway/SavingStatus";

type RedactionLogContentProps = {
  redactionHighlights: IPdfHighlight[];
  savingStatus: SavingStatus;
  message?: string;
  handleClose?: () => void;
};

const defaultValues = {
  select1: "2",
  select2: "1",
  select3: "1",
  select4: "1",
  select5: "1",
  textArea: "",
};
export const RedactionLogContent: React.FC<RedactionLogContentProps> = ({
  message,
  savingStatus,
  handleClose,
  redactionHighlights,
}) => {
  const {
    handleSubmit,
    getValues,
    formState: { errors },
    control,
  } = useForm({ defaultValues });
  const [showSelectEdits, setShowSelectEdits] = useState<string[]>([]);
  const handleEditButtonClick = (id: string) => {
    console.log("id>>>>", id);
    setShowSelectEdits([...showSelectEdits, id]);
  };

  const redactionLogGuidanceContent = () => {
    return (
      <div className={classes.redactionLogGuidanceWrapper}>
        <p className={classes.redactionLogGuidanceTitle}>
          Redaction Log Guidance
        </p>
        <ul className={classes.redactionLogGuidanceList}>
          <li>
            This popup allows the capture of details which will be recorded into
            the Redaction Log automatically
          </li>
          <li>
            Once added, if an entry needs editing or deleting, this should be
            done in the Redaction log
          </li>
          <li>
            Contact with the Investigative Agency or the CPS is not automatic -
            you should contact any such bodies yourself
          </li>
        </ul>
      </div>
    );
  };

  const supportingNotesGuidanceContent = () => {
    return (
      <div className={classes.supportingNotesGuidanceWrapper}>
        <p className={classes.supportingNotesGuidanceTitle}>
          Guidance on supporting notes
        </p>
        <ul className={classes.supportingNotesGuidanceList}>
          <li>
            Detail the redaction issue identified, e.g. Statement of XX
            (Initials) DOB redacted
          </li>
          <li>Avoid recording full names</li>
          <li>Do not record sensitive personal data</li>
          <li>Supporting notes optional - 400 characters maximum</li>
        </ul>
      </div>
    );
  };

  return (
    <div className={classes.modalContent}>
      {savingStatus === "saving" && (
        <div className={classes.savingBanner}>
          <span>Saving redactions...</span>
        </div>
      )}

      {savingStatus === "saved" && (
        <div className={classes.savedBanner}>
          <span>Redactions successfully saved</span>
        </div>
      )}
      <div className={classes.modalHeadWrapper}>
        <div className={classes.modalTitleWrapper}>
          <h1 className={classes.modalContentHeading}>
            45AA209820/1 - Redaction Log
          </h1>

          <Guidance
            name="Redaction log Guidance"
            className={classes.redactionLogGuidance}
          >
            {redactionLogGuidanceContent()}
          </Guidance>
        </div>

        <form
          className={classes.underRedactionForm}
          onSubmit={() => {
            console.log("hellooo");
          }}
        >
          <div className={classes.selectInputWrapper}>
            <section className={classes.selectSection}>
              <Controller
                name="select1"
                control={control}
                // rules={{
                //   required: true,
                //   validate: {
                //     test1: (value) => {
                //       if (value === "1") return false;
                //     },
                //     test2: (value) => {
                //       if (value === "2") return false;
                //     },
                //   },
                // }}
                render={({ field }) => {
                  return (
                    <>
                      {!showSelectEdits.includes("select1") ? (
                        <div className={classes.editBtnWrapper}>
                          <span>CPS Area or Central Casework Division:</span>
                          <EditButton
                            id="select1"
                            callBackFn={handleEditButtonClick}
                            value={getValues("select1")}
                          />
                        </div>
                      ) : (
                        <Select
                          {...field}
                          label={{
                            htmlFor: "select-cps-area",
                            children: "CPS Area or Central Casework Division:",
                            className: classes.selectLabel,
                          }}
                          id="select-cps-area"
                          data-testid="select-cps-area"
                          formGroup={{
                            className: classes.select,
                          }}
                          items={[
                            { children: "value1", value: "1" as const },
                            { children: "value2", value: "2" as const },
                          ]}
                        />
                      )}
                    </>
                  );
                }}
              />
              {errors.select1?.type === "required" && (
                <p className={classes.errorMsg}>This is a required field.</p>
              )}
              {errors.select1 && (
                <p
                  className={classes.errorMsg}
                >{`an error of type: ${errors.select1?.type}`}</p>
              )}
            </section>
            <section className={classes.selectSection}>
              <Controller
                name="select2"
                control={control}
                render={({ field }) => {
                  return (
                    <>
                      {!showSelectEdits.includes("select2") ? (
                        <div className={classes.editBtnWrapper}>
                          <span>CPS Business unit:</span>
                          <EditButton
                            id="select2"
                            callBackFn={handleEditButtonClick}
                            value={getValues("select2")}
                          />
                        </div>
                      ) : (
                        <Select
                          {...field}
                          label={{
                            htmlFor: "select-cps-bu",
                            children: "CPS Business unit:",
                            className: classes.selectLabel,
                          }}
                          id="select-cps-bu"
                          data-testid="select-cps-bu"
                          formGroup={{
                            className: classes.select,
                          }}
                          items={[
                            { children: "value1", value: "1" as const },
                            { children: "value2", value: "2" as const },
                          ]}
                        />
                      )}
                    </>
                  );
                }}
              />
              {errors.select2?.type === "required" && (
                <p className={classes.errorMsg}>This is a required field.</p>
              )}
              {errors.select2 && (
                <p
                  className={classes.errorMsg}
                >{`an error of type: ${errors.select2?.type}`}</p>
              )}
            </section>
            <section className={classes.selectSection}>
              <Controller
                name="select3"
                control={control}
                render={({ field }) => {
                  return (
                    <>
                      {!showSelectEdits.includes("select3") ? (
                        <div className={classes.editBtnWrapper}>
                          <span>Investigative Agency:</span>
                          <EditButton
                            id="select3"
                            callBackFn={handleEditButtonClick}
                            value={getValues("select3")}
                          />
                        </div>
                      ) : (
                        <Select
                          {...field}
                          label={{
                            htmlFor: "select-cps-ia",
                            children: "Investigative Agency:",
                            className: classes.selectLabel,
                          }}
                          id="select-cps-ia"
                          data-testid="select-cps-ia"
                          formGroup={{
                            className: classes.select,
                          }}
                          items={[
                            { children: "value1", value: "1" as const },
                            { children: "value2", value: "2" as const },
                          ]}
                        />
                      )}
                    </>
                  );
                }}
              />
              {errors.select3?.type === "required" && (
                <p className={classes.errorMsg}>This is a required field.</p>
              )}
              {errors.select3 && (
                <p
                  className={classes.errorMsg}
                >{`an error of type: ${errors.select3?.type}`}</p>
              )}
            </section>
            <section className={classes.selectSection}>
              <Controller
                name="select4"
                control={control}
                render={({ field }) => {
                  return (
                    <>
                      {!showSelectEdits.includes("select4") ? (
                        <div className={classes.editBtnWrapper}>
                          <span>Charge Status:</span>
                          <EditButton
                            id="select4"
                            callBackFn={handleEditButtonClick}
                            value={getValues("select4")}
                          />
                        </div>
                      ) : (
                        <Select
                          {...field}
                          label={{
                            htmlFor: "select-cps-cs",
                            children: "Charge Status:",
                            className: classes.selectLabel,
                          }}
                          id="select-cps-cs"
                          data-testid="select-cps-cs"
                          formGroup={{
                            className: classes.select,
                          }}
                          items={[
                            { children: "value1", value: "1" as const },
                            { children: "value2", value: "2" as const },
                          ]}
                        />
                      )}
                    </>
                  );
                }}
              />
              {errors.select4?.type === "required" && (
                <p className={classes.errorMsg}>This is a required field.</p>
              )}
              {errors.select4 && (
                <p
                  className={classes.errorMsg}
                >{`an error of type: ${errors.select4?.type}`}</p>
              )}
            </section>
            <section className={classes.selectSection}>
              <Controller
                name="select5"
                control={control}
                render={({ field }) => {
                  return (
                    <>
                      {!showSelectEdits.includes("select5") ? (
                        <div className={classes.editBtnWrapper}>
                          <span>Document Type:</span>
                          <EditButton
                            id="select5"
                            callBackFn={handleEditButtonClick}
                            value={getValues("select5")}
                          />
                        </div>
                      ) : (
                        <Select
                          {...field}
                          label={{
                            htmlFor: "select-cps-dt",
                            children: "Document Type:",
                            className: classes.selectLabel,
                          }}
                          id="select-cps-dt"
                          data-testid="select-cps-dt"
                          formGroup={{
                            className: classes.select,
                          }}
                          items={[
                            { children: "value1", value: "1" as const },
                            { children: "value2", value: "2" as const },
                          ]}
                        />
                      )}
                    </>
                  );
                }}
              />
              {errors.select5?.type === "required" && (
                <p className={classes.errorMsg}>This is a required field.</p>
              )}
              {errors.select5 && (
                <p
                  className={classes.errorMsg}
                >{`an error of type: ${errors.select5?.type}`}</p>
              )}
            </section>
          </div>
        </form>
      </div>
      <div className={classes.modalBodyWrapper}>
        <section>
          <UnderRedactionContent
            documentName="ABC_MG3"
            redactionHighlights={redactionHighlights}
          />
        </section>
        <section className={classes.textAreaSection}>
          <Guidance
            name="Guidance on supporting notes"
            className={classes.supportingNotesGuidance}
          >
            {supportingNotesGuidanceContent()}
          </Guidance>

          <TextArea
            value={"abc"}
            onChange={() => {}}
            name="more-details"
            id="more-details"
            data-testid="report-issue-more-details"
            label={{
              children: (
                <span className={classes.textAreaLabel}>
                  Supporting notes (optional)
                </span>
              ),
            }}
          />
        </section>
      </div>
      <div className={classes.btnWrapper}>
        <Button
          disabled={savingStatus === "saving"}
          type="submit"
          className={classes.saveBtn}
          onClick={handleSubmit((data) => console.log(data))}
          data-testid="btn-feedback-modal-ok"
        >
          Save and Close
        </Button>
      </div>
    </div>
  );
};
