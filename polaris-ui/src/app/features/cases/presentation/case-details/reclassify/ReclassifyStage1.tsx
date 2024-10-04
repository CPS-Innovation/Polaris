import { useMemo, useRef, useEffect } from "react";
import {
  Select,
  ErrorSummary,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { FormDataErrors } from "./data/FormDataErrors";
import classes from "./Reclassify.module.scss";

type ReclassifyStage1Props = {
  formDataErrors: FormDataErrors;
  presentationTitle: string;
  currentDocTypeId: number | null;
};

export const ReclassifyStage1: React.FC<ReclassifyStage1Props> = ({
  presentationTitle,
  formDataErrors,
  currentDocTypeId,
}) => {
  const reclassifyContext = useReClassifyContext()!;
  const errorSummaryRef = useRef(null);

  const { state, dispatch } = reclassifyContext;

  const currentClassificationVariant = useMemo(() => {
    return state.materialTypeList.find(
      (material) => material.typeId === currentDocTypeId
    )?.newClassificationVariant;
  }, [state.materialTypeList, currentDocTypeId]);

  useEffect(() => {
    if (formDataErrors.documentTypeErrorText && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [formDataErrors]);

  const docTypesValues = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Choose document type",
      disabled: true,
    };
    const mappedDocTypeValues = state.materialTypeList.map(
      ({ typeId, description, newClassificationVariant }) => {
        let disabled = false;
        if (typeId === currentDocTypeId) disabled = true;
        if (
          currentClassificationVariant === "Exhibit" &&
          newClassificationVariant === "Exhibit"
        )
          disabled = true;
        return {
          value: typeId,
          children: description,
          disabled: disabled,
        };
      }
    );
    return [defaultValue, ...mappedDocTypeValues];
  }, [state.materialTypeList, currentDocTypeId, currentClassificationVariant]);

  const handleDocTypeChange = (value: string) => {
    dispatch({ type: "UPDATE_DOCUMENT_TYPE", payload: { id: value } });
  };
  return (
    <div>
      <h1>What type of document is this?</h1>
      {formDataErrors.documentTypeErrorText && (
        <div
          ref={errorSummaryRef}
          tabIndex={-1}
          className={classes.errorSummaryWrapper}
        >
          <ErrorSummary
            data-testid={"doctypeId-error-summary"}
            className={classes.errorSummary}
            errorList={[
              {
                reactListKey: "1",
                children: formDataErrors.documentTypeErrorText,
                href: "#reclassify-document-type",
              },
            ]}
          />
        </div>
      )}
      <Select
        errorMessage={
          formDataErrors.documentTypeErrorText
            ? {
                children: formDataErrors.documentTypeErrorText,
              }
            : undefined
        }
        label={{
          htmlFor: "reclassify-document-type",
          children: (
            <span>
              Select the document type for{" "}
              <strong className={classes.highlight}>{presentationTitle}</strong>
            </span>
          ),
        }}
        id="reclassify-document-type"
        data-testid="reclassify-document-type"
        items={docTypesValues}
        value={state.newDocTypeId}
        onChange={(ev) => handleDocTypeChange(ev.target.value)}
      />
    </div>
  );
};
