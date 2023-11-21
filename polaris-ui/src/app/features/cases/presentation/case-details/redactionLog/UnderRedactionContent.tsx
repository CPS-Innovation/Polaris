import {
  Select,
  Input,
  TextArea,
  Button,
} from "../../../../../common/presentation/components";
import classes from "./UnderRedactionContent.module.scss";
import { useForm, Controller } from "react-hook-form";

type UnderRedactionContentProps = {
  message?: string;
  handleClose?: () => void;
};

const defaultValues = {
  select1: "1",
  select2: "1",
  select3: "1",
  select4: "1",
  select5: "1",
  textArea: "",
};
export const UnderRedactionContent: React.FC<UnderRedactionContentProps> = ({
  message,
  handleClose,
}) => {
  const {
    handleSubmit,
    formState: { errors },
    control,
  } = useForm({ defaultValues });
  return (
    <div className={classes.modalContent}>
      <h1 className={classes.modalContentHeading}>
        45AA209820/1 - Redaction Log
      </h1>
      <form
        className={classes.underRedactionForm}
        // onSubmit={handleSubmit((data) => console.log(data))}
      >
        <div className={classes.selectInputWrapper}>
          <section className={classes.selectSection}>
            <Controller
              name="select1"
              control={control}
              render={({ field }) => {
                return (
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
                );
              }}
            />
            {errors.select1?.type === "required" && (
              <p className="errorMsg">This is a required field.</p>
            )}
            {errors.select1 && (
              <p className="errorMsg">{`an error of type: ${errors.select1?.type}`}</p>
            )}
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="select2"
              control={control}
              render={({ field }) => {
                return (
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
                );
              }}
            />
            {errors.select1?.type === "required" && (
              <p className="errorMsg">This is a required field.</p>
            )}
            {errors.select1 && (
              <p className="errorMsg">{`an error of type: ${errors.select1?.type}`}</p>
            )}
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="select3"
              control={control}
              render={({ field }) => {
                return (
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
                );
              }}
            />
            {errors.select1?.type === "required" && (
              <p className="errorMsg">This is a required field.</p>
            )}
            {errors.select1 && (
              <p className="errorMsg">{`an error of type: ${errors.select1?.type}`}</p>
            )}
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="select4"
              control={control}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    label={{
                      htmlFor: "select-cps-cs",
                      children: "Charge status:",
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
                );
              }}
            />
            {errors.select1?.type === "required" && (
              <p className="errorMsg">This is a required field.</p>
            )}
            {errors.select1 && (
              <p className="errorMsg">{`an error of type: ${errors.select1?.type}`}</p>
            )}
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="select5"
              control={control}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    label={{
                      htmlFor: "select-cps-dt",
                      children: "Document type:",
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
                );
              }}
            />
            {errors.select1?.type === "required" && (
              <p className="errorMsg">This is a required field.</p>
            )}
            {errors.select1 && (
              <p className="errorMsg">{`an error of type: ${errors.select1?.type}`}</p>
            )}
          </section>
        </div>
        <section className={classes.textAreaSection}>
          <Controller
            name="textArea"
            // rules={{
            //   required: true,
            //   validate: {
            //     test1: (value) => {
            //       if (value.length > 2 && value.length < 4) return false;
            //     },
            //     test2: (value) => {
            //       if (value.length > 3) return false;
            //     },
            //   },
            // }}
            control={control}
            render={({ field }) => {
              console.log("field>>>>>111", field);
              return (
                <TextArea
                  {...field}
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
              );
            }}
          />
          {errors.textArea?.type === "required" && (
            <p className="errorMsg">This is a required field.</p>
          )}
          {errors.textArea && (
            <p className="errorMsg">{`an error of type: ${errors.textArea?.type}`}</p>
          )}
        </section>
        {/* <input type="submit" /> */}
        <div className={classes.btnWrapper}>
          <Button
            type="submit"
            className={classes.saveBtn}
            onClick={handleSubmit((data) => console.log(data))}
            data-testid="btn-feedback-modal-ok"
          >
            Save and Close
          </Button>
        </div>
      </form>
    </div>
  );
};
