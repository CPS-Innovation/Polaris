import { useState, useEffect, useRef, useCallback } from "react";
import {
  LinkButton,
  Button,
  Modal,
  NotificationBanner,
  Spinner,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyStage1 } from "./ReclassifyStage1";
import { ReclassifyStage2 } from "./ReclassifyStage2";
import { ReclassifyStage3 } from "./ReclassifyStage3";
import { FormDataErrors } from "./data/FormDataErrors";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { StatementWitnessNumber } from "./data/StatementWitnessNumber";
import { ReclassifySaveData } from "./data/ReclassifySaveData";
import { validateDate } from "./utils/dateValidation";
import {
  handleTextValidation,
  EXHIBIT_TEXT_VALIDATION_REGEX,
  EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX,
} from "./utils/textValidation";
import classes from "./Reclassify.module.scss";

type ReclassifyStagesProps = {
  documentId: string;
  currentDocTypeId: number | null;
  presentationTitle: string;
  reclassifiedDocumentUpdate?: boolean;
  handleCloseReclassify: (documentId: string) => void;
  getMaterialTypeList: () => Promise<MaterialType[]>;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  getWitnessStatementNumbers: (
    witnessId: number
  ) => Promise<StatementWitnessNumber[]>;
  handleSubmitReclassify: (
    documentId: string,
    data: ReclassifySaveData
  ) => Promise<boolean>;
  handleReclassifyTracking: (
    name: string,
    properties: Record<string, any>
  ) => void;
};

const MAX_LENGTH = 252;

export const ReclassifyStages: React.FC<ReclassifyStagesProps> = ({
  documentId,
  currentDocTypeId,
  presentationTitle,
  reclassifiedDocumentUpdate,
  handleCloseReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  handleSubmitReclassify,
  handleReclassifyTracking,
}) => {
  const backButtonRef = useRef(null);
  const errorTextsInitialValue: FormDataErrors = {
    documentTypeErrorText: "",
    documentNewNameErrorText: "",
    exhibitItemNameErrorText: "",
    exhibitReferenceErrorText: "",
    otherExhibitProducerErrorText: "",
    statementWitnessErrorText: "",
    statementDayErrorText: "",
    statementMonthErrorText: "",
    statementYearErrorText: "",
    statementDateErrorText: "",
    statementNumberErrorText: "",
  };
  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>(
    errorTextsInitialValue
  );

  const [loading, setLoading] = useState(false);
  const reclassifyContext = useReClassifyContext()!;

  const { state, dispatch } = reclassifyContext;

  const validateData = () => {
    if (state.reClassifyStage === "stage3") {
      return true;
    }
    const {
      reclassifyVariant,
      statementWitnessNumbers,
      formData: {
        documentRenameStatus,
        documentNewName,
        exhibitItemName,
        exhibitReference,
        exhibitProducerId,
        exhibitOtherProducerValue,
        statementWitnessId,
        statementDay,
        statementMonth,
        statementYear,
        statementNumber,
      },
    } = reclassifyContext.state;

    const errorTexts: FormDataErrors = { ...errorTextsInitialValue };

    const handleRenameValidation = () => {
      if (documentRenameStatus === "YES") {
        if (!documentNewName) {
          errorTexts.documentNewNameErrorText = "New name should not be empty";
        }
        if (documentNewName === presentationTitle) {
          errorTexts.documentNewNameErrorText =
            "New name should be different from current name";
        }
        if (documentNewName.length > MAX_LENGTH) {
          errorTexts.documentNewNameErrorText = `New name must be ${MAX_LENGTH} characters or less`;
        }
      }
    };

    const exhibitItemNameValidation = () => {
      if (!exhibitItemName) {
        errorTexts.exhibitItemNameErrorText =
          "Exhibit item should not be empty";
        return;
      }
      const characterErrorText = handleTextValidation(
        exhibitItemName,
        EXHIBIT_TEXT_VALIDATION_REGEX
      );
      if (characterErrorText) {
        errorTexts.exhibitItemNameErrorText = `Exhibit item should not contain ${characterErrorText}`;
      }
      if (exhibitItemName.length > MAX_LENGTH) {
        errorTexts.exhibitItemNameErrorText = `Exhibit item must be ${MAX_LENGTH} characters or less`;
      }
    };

    const exhibitReferenceValidation = () => {
      if (!exhibitReference) {
        errorTexts.exhibitReferenceErrorText =
          "Exhibit reference should not be empty";
        return;
      }
      const characterErrorText = handleTextValidation(
        exhibitReference,
        EXHIBIT_TEXT_VALIDATION_REGEX
      );

      if (characterErrorText) {
        errorTexts.exhibitReferenceErrorText = `Exhibit reference should not contain ${characterErrorText}`;
      }
    };

    const exhibitNewProducerIdValidation = () => {
      if (exhibitProducerId === "other") {
        if (!exhibitOtherProducerValue) {
          errorTexts.otherExhibitProducerErrorText = `Exhibit new producer or witness should not be empty`;
          return;
        }
        const characterErrorText = handleTextValidation(
          exhibitOtherProducerValue,
          EXHIBIT_PRODUCER_TEXT_VALIDATION_REGEX
        );
        if (characterErrorText) {
          errorTexts.otherExhibitProducerErrorText = `Exhibit new producer or witness text should not contain ${characterErrorText}`;
        }
      }
    };

    const handleExhibitValidation = () => {
      exhibitItemNameValidation();
      exhibitReferenceValidation();
      exhibitNewProducerIdValidation();
    };

    const handleStatementValidation = () => {
      const result = validateDate(
        +statementDay,
        +statementMonth,
        +statementYear
      );

      if (!statementWitnessId) {
        errorTexts.statementWitnessErrorText =
          "Statement witness should not be empty";
      }
      if (!statementNumber) {
        errorTexts.statementNumberErrorText =
          "Statement number should not be empty";
      }
      if (
        statementWitnessNumbers[statementWitnessId]?.includes(+statementNumber)
      ) {
        errorTexts.statementNumberErrorText = `Statement number ${statementNumber} already exist`;
      }
      if (result.errors.includes("invalid day")) {
        errorTexts.statementDayErrorText = "invalid day";
        errorTexts.statementDateErrorText =
          "Statement date must be a real date";
      }
      if (result.errors.includes("invalid month")) {
        errorTexts.statementMonthErrorText = "invalid month";
        errorTexts.statementDateErrorText =
          "Statement date must be a real date";
      }
      if (result.errors.includes("invalid year")) {
        errorTexts.statementYearErrorText = "invalid year";
        errorTexts.statementDateErrorText =
          "Statement date must be a real date";
      }
    };

    if (state.reClassifyStage === "stage1") {
      if (!state.newDocTypeId) {
        errorTexts.documentTypeErrorText =
          "New document type should not be empty";
      }
    }

    if (state.reClassifyStage === "stage2") {
      handleRenameValidation();
      if (reclassifyVariant === "Exhibit") {
        handleExhibitValidation();
      }
      if (reclassifyVariant === "Statement") {
        handleStatementValidation();
      }
    }

    setFormDataErrors(errorTexts);
    const validErrors = Object.keys(errorTexts).filter(
      (key) => errorTexts[key as keyof FormDataErrors]
    );

    return !validErrors.length;
  };

  const getMappedSaveData = () => {
    const { newDocTypeId, materialTypeList, formData } = state;
    const reclassificationType = materialTypeList.find(
      (type) => type.typeId === +newDocTypeId
    )?.newClassificationVariant!;
    const used = formData.documentUsedStatus === "YES";

    const saveData = {
      documentId: parseInt(documentId.replace(/\D/g, "")),
      documentTypeId: +newDocTypeId!,
      immediate:
        state.reclassifyVariant === "Immediate"
          ? {
              documentName:
                formData.documentRenameStatus === "YES"
                  ? formData.documentNewName
                  : null,
            }
          : null,
      other:
        state.reclassifyVariant === "Other"
          ? {
              documentName:
                formData.documentRenameStatus === "YES"
                  ? formData.documentNewName
                  : null,
              used,
            }
          : null,
      statement:
        state.reclassifyVariant === "Statement"
          ? {
              used,
              witnessId: +formData.statementWitnessId!,
              statementNo: +formData.statementNumber!,
              date:
                reclassificationType === "Statement"
                  ? `${formData.statementYear}-${formData.statementMonth}-${formData.statementDay}`
                  : "",
            }
          : null,
      exhibit:
        state.reclassifyVariant === "Exhibit"
          ? {
              used,
              existingProducerOrWitnessId:
                formData.exhibitProducerId &&
                formData.exhibitProducerId !== "other"
                  ? +formData.exhibitProducerId
                  : null,
              newProducer:
                formData.exhibitProducerId === "other"
                  ? formData.exhibitOtherProducerValue
                  : null,
              item: formData.exhibitItemName,
              reference: formData.exhibitReference,
            }
          : null,
    };
    return saveData;
  };

  const handleContinueBtnClick = () => {
    const validData = validateData();
    if (!validData) return;
    if (backButtonRef.current)
      (backButtonRef.current as HTMLButtonElement).focus();
    if (state.reClassifyStage === "stage1") {
      dispatch({
        type: "UPDATE_CLASSIFY_STAGE",
        payload: { newStage: "stage2" },
      });
      dispatch({
        type: "RESET_FORM_DATA",
        payload: { presentationTitle: presentationTitle },
      });
      return;
    }

    if (state.reClassifyStage === "stage2") {
      dispatch({
        type: "UPDATE_CLASSIFY_STAGE",
        payload: { newStage: "stage3" },
      });
    }
  };

  const handleBackBtnClick = () => {
    if (state.reClassifyStage === "stage1") {
      closeReclassify();
      return;
    }

    if (state.reClassifyStage === "stage2") {
      dispatch({
        type: "UPDATE_CLASSIFY_STAGE",
        payload: { newStage: "stage1" },
      });
      return;
    }
    if (state.reClassifyStage === "stage3") {
      dispatch({
        type: "UPDATE_CLASSIFY_STAGE",
        payload: { newStage: "stage2" },
      });
    }
  };

  const handleAcceptAndSave = async () => {
    const saveData: ReclassifySaveData = getMappedSaveData();
    dispatch({
      type: "UPDATE_RECLASSIFY_SAVE_STATUS",
      payload: { value: "saving" },
    });
    handleReclassifyTracking("Save Reclassify", saveData);
    const result = await handleSubmitReclassify(documentId, saveData);
    if (result) {
      dispatch({
        type: "UPDATE_RECLASSIFY_SAVE_STATUS",
        payload: { value: "success" },
      });
      if (reclassifiedDocumentUpdate === undefined) {
        closeReclassify();
        return;
      }
    }

    if (!result) {
      dispatch({
        type: "UPDATE_RECLASSIFY_SAVE_STATUS",
        payload: { value: "failure" },
      });
      handleReclassifyTracking("Save Reclassify Error", saveData);
    }
  };

  const handleCloseErrorModal = () => {
    dispatch({
      type: "UPDATE_RECLASSIFY_SAVE_STATUS",
      payload: { value: "initial" },
    });
  };

  const closeReclassify = useCallback(() => {
    handleCloseReclassify(documentId);
  }, [handleCloseReclassify, documentId]);

  const renderActionButtons = () => {
    if (state.reClassifyStage !== "stage3") {
      return (
        <>
          <Button
            onClick={handleContinueBtnClick}
            disabled={
              state.reClassifyStage === "stage2" &&
              state.reclassifyVariant === "Statement" &&
              !state.statementWitness?.length
            }
          >
            Continue
          </Button>
          <LinkButton className={classes.btnCancel} onClick={closeReclassify}>
            Cancel
          </LinkButton>
        </>
      );
    }
    return (
      <Button
        onClick={handleAcceptAndSave}
        disabled={
          state.reClassifySaveStatus === "saving" ||
          state.reClassifySaveStatus === "success"
        }
      >
        Accept and save
      </Button>
    );
  };

  const getAriaMainRoleDescription = () => {
    if (state.reClassifyStage === "stage1")
      return "What type of document is this?";
    if (state.reClassifyStage === "stage3") return "Check your answers";
    if (
      state.reClassifyStage === "stage2" &&
      state.reclassifyVariant === "Exhibit"
    )
      return "Enter the exhibit details";
    if (
      state.reClassifyStage === "stage2" &&
      state.reclassifyVariant === "Statement"
    )
      return "Enter the statement details";
    return "Enter the document details";
  };

  useEffect(() => {
    const fetchDataOnMount = async () => {
      if (state.materialTypeList.length) return;
      setLoading(true);
      try {
        const result = await getMaterialTypeList();
        dispatch({
          type: "ADD_MATERIAL_TYPE_LIST",
          payload: { materialList: result },
        });
      } catch (error) {
        console.error("Error fetching data:", error);
      } finally {
        setLoading(false);
        if (backButtonRef.current)
          (backButtonRef.current as HTMLButtonElement).focus();
      }
    };

    fetchDataOnMount();
  }, []);

  useEffect(() => {
    if (reclassifiedDocumentUpdate) {
      setTimeout(() => {
        closeReclassify();
      }, 2000);
    }
  }, [reclassifiedDocumentUpdate, closeReclassify, documentId]);

  if (loading) {
    return <div>loading data</div>;
  }
  return (
    <div
      className={classes.reClassifyStages}
      role="main"
      aria-describedby="main-description"
    >
      <span id="main-description" className={classes.visuallyHidden}>
        {getAriaMainRoleDescription()}
      </span>
      <LinkButton
        className={classes.backBtn}
        onClick={handleBackBtnClick}
        disabled={
          state.reClassifySaveStatus === "saving" ||
          state.reClassifySaveStatus === "success"
        }
        ref={backButtonRef}
      >
        Back
      </LinkButton>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <div aria-live="polite" className={classes.visuallyHidden}>
            {(state.reClassifySaveStatus === "saving" ||
              (state.reClassifySaveStatus === "success" &&
                !reclassifiedDocumentUpdate)) && (
              <span>Saving to CMS. Please wait</span>
            )}
            {reclassifiedDocumentUpdate && <span>Successfully saved</span>}
          </div>
          {(state.reClassifySaveStatus === "saving" ||
            state.reClassifySaveStatus === "success") && (
            <NotificationBanner className={classes.notificationBanner}>
              <div className={classes.bannerContent}>
                <div className={classes.spinnerWrapper}>
                  <Spinner diameterPx={25} ariaLabel={"spinner-animation"} />
                </div>
                <p className={classes.notificationBannerText}>
                  Saving to CMS. Please wait.
                </p>
              </div>
            </NotificationBanner>
          )}

          {state.reClassifyStage === "stage1" && (
            <ReclassifyStage1
              currentDocTypeId={currentDocTypeId}
              presentationTitle={presentationTitle}
              formDataErrors={formDataErrors}
            />
          )}
          {state.reClassifyStage === "stage2" && (
            <ReclassifyStage2
              presentationTitle={presentationTitle}
              formDataErrors={formDataErrors}
              getExhibitProducers={getExhibitProducers}
              getStatementWitnessDetails={getStatementWitnessDetails}
              getWitnessStatementNumbers={getWitnessStatementNumbers}
            />
          )}

          {state.reClassifyStage === "stage3" && (
            <ReclassifyStage3
              presentationTitle={presentationTitle}
              backButtonRef={backButtonRef}
            />
          )}

          <div className={classes.btnWrapper}>{renderActionButtons()}</div>
        </div>
      </div>
      {state.reClassifySaveStatus === "failure" && (
        <Modal
          isVisible
          handleClose={() => {
            handleCloseErrorModal();
          }}
          type="alert"
          ariaLabel="Save reclassification error modal"
          ariaDescription="Failed to save the reclassification. Please try again later"
        >
          <div className={classes.alertContent}>
            <h1 className="govuk-heading-l">Something went wrong</h1>
            <p>Failed to save the reclassification. Please try again later</p>
            <div className={classes.actionButtonsWrapper}>
              <Button
                onClick={() => {
                  handleCloseErrorModal();
                }}
                data-testid="btn-reclassify-error-ok"
              >
                Ok
              </Button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
