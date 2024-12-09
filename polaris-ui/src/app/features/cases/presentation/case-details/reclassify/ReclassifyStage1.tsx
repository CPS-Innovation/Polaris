import { useMemo, useRef, useEffect, useState } from "react";
import {
  LinkButton,
  Select,
  ErrorSummary,
  Spinner,
  NotificationBanner
} from "../../../../../common/presentation/components";
import classes from "./Reclassify.module.scss";
import { ReclassifyStage2 } from './ReclassifyStage2'

import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyVariant } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { StatementWitnessNumber } from "./data/StatementWitnessNumber";
import { statementNumberText } from "./utils/statementNumberText";
import { FormDataErrors } from "./data/FormDataErrors";

type ReclassifyStage1Props = {
  formDataErrors: FormDataErrors;
  presentationTitle: string;
  currentDocTypeId: number | null;
  handleBackBtnClick: () => void;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  getWitnessStatementNumbers: (
    witnessId: number
  ) => Promise<StatementWitnessNumber[]>;
  handleLookUpDataError: (errorMessage: string) => void;
  reclassifiedDocumentUpdate?: boolean
};

export const ReclassifyStage1: React.FC<ReclassifyStage1Props> = ({
  presentationTitle,
  formDataErrors,
  currentDocTypeId,
  handleBackBtnClick,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  handleLookUpDataError,
  reclassifiedDocumentUpdate
}) => {
  const reclassifyContext = useReClassifyContext()!;
  const errorSummaryRef = useRef(null);
  const backButtonRef = useRef(null);

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

  useEffect(() => {
    if (backButtonRef.current)
      (backButtonRef.current as HTMLButtonElement).focus();
  }, []);

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
    <div role="main" aria-labelledby="main-description">
      <LinkButton
        className={classes.backBtn}
        onClick={handleBackBtnClick}
        ref={backButtonRef}
        disabled={
          state.reClassifySaveStatus === 'saving' ||
            state.reClassifySaveStatus === "success" ? true : false
        }
      >
        Back
      </LinkButton>
      <div aria-live="polite" className={classes.visuallyHidden}>
        {(
          ((state.reClassifySaveStatus === 'saving' || state.reClassifySaveStatus === "success") &&
            !reclassifiedDocumentUpdate)) && (
            <span>Saving to CMS. Please wait</span>
          )}
        {reclassifiedDocumentUpdate && <span>Successfully saved</span>}
      </div>
      {(state.reClassifySaveStatus === 'saving' ||
        state.reClassifySaveStatus === "success") && (
          <NotificationBanner className={classes.notificationBanner}>
            <div
              className={classes.bannerContent}
              data-testid="div-notification-banner"
            >
              <div className={classes.spinnerWrapper}>
                <Spinner diameterPx={25} ariaLabel={"spinner-animation"} />
              </div>
              <p className={classes.notificationBannerText}>
                Saving to CMS. Please wait.
              </p>
            </div>
          </NotificationBanner>
        )}
      <h1 id="main-description" className="govuk-heading-l">What type of document is this?</h1>
      {formDataErrors.documentTypeErrorText && (
        <div
          ref={errorSummaryRef}
          tabIndex={-1}
          className={classes.errorSummaryWrapper}
        >
          <ErrorSummary
            data-testid={"reclassify-doctypeId-error-summary"}
            className={classes.errorSummary}
            errorList={[
              {
                reactListKey: "1",
                children: formDataErrors.documentTypeErrorText,
                href: "#reclassify-document-type",
                "data-testid": "reclassify-document-type-link",
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
        disabled={
          state.reClassifySaveStatus === 'saving' || state.reClassifySaveStatus === "success" ? true : false
        }
      />
      {state?.newDocTypeId ? (
        <ReclassifyStage2
          presentationTitle={presentationTitle}
          formDataErrors={formDataErrors}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
          getWitnessStatementNumbers={getWitnessStatementNumbers}
          handleBackBtnClick={handleBackBtnClick}
          handleLookUpDataError={handleLookUpDataError}
        />
      ) : ''}
    </div>
  );
};