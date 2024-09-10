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
  console.log("reclassifiedDocumentUpdate>>>", reclassifiedDocumentUpdate);
  const continueButtonRef = useRef(null);
  const errorTextsInitialValue: FormDataErrors = {
    documentTypeErrorText: "",
    documentNewNameErrorText: "",
    exhibitItemNameErrorText: "",
    otherExhibitProducerErrorText: "",
    exhibitReferenceErrorText: "",
    statementWitnessErrorText: "",
    statementNumberErrorText: "",
    statementDayErrorText: "",
    statementMonthErrorText: "",
    statementYearErrorText: "",
    statementDateErrorText: "",
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

    if (state.reClassifyStage === "stage1") {
      if (!state.newDocTypeId) {
        errorTexts.documentTypeErrorText =
          "New document type should not be empty";
      }
      // if (state.newDocTypeId === documentId) {
      //   errorTexts.documentTypeErrorText =
      //     "New document type should be different from current one";
      // }
    }

    if (state.reClassifyStage === "stage2") {
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
      if (reclassifyVariant === "Exhibit") {
        if (!exhibitItemName) {
          errorTexts.exhibitItemNameErrorText =
            "Exhibit item name should not be empty";
        }
        if (!exhibitReference) {
          errorTexts.exhibitReferenceErrorText =
            "Exhibit reference should not be empty";
        }
        if (exhibitItemName.length > MAX_LENGTH) {
          errorTexts.exhibitItemNameErrorText = `Exhibit item name must be ${MAX_LENGTH} characters or less`;
        }
        if (exhibitProducerId === "other" && !exhibitOtherProducerValue) {
          errorTexts.otherExhibitProducerErrorText = `Exhibit existing producer or witness should not be empty`;
        }
      }

      if (reclassifyVariant === "Statement") {
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
          statementWitnessNumbers[statementWitnessId]?.includes(
            +statementNumber
          )
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
      }
    }
    setFormDataErrors(errorTexts);
    const validErrors = Object.keys(errorTexts).filter(
      (key) => errorTexts[key as keyof FormDataErrors]
    );

    console.log("validErrors>>", validErrors.length);
    return !validErrors.length;
  };

  const getMappedSaveData = () => {
    const { newDocTypeId, materialTypeList, formData } = state;
    const reclassificationType = materialTypeList.find(
      (type) => type.typeId === +newDocTypeId
    )?.newClassificationVariant!;
    const used = formData.documentUsedStatus === "YES" ? true : false;

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
                  ? `${formData.statementDay}-${formData.statementMonth}-${formData.statementYear}`
                  : "",
            }
          : null,
      exhibit:
        state.reclassifyVariant === "Exhibit"
          ? {
              used,
              existingProducerOrWitnessId:
                formData.exhibitProducerId !== "other"
                  ? +formData.exhibitProducerId ?? null
                  : null,
              newProducer: formData.exhibitOtherProducerValue,
              item: formData.exhibitItemName,
              reference: formData.exhibitReference,
            }
          : null,
    };
    return saveData;
  };

  const handleContinueBtnClick = () => {
    if (continueButtonRef.current)
      (continueButtonRef.current as HTMLButtonElement).blur();
    const validData = validateData();
    if (!validData) return;
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
      return;
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
      return;
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
            ref={continueButtonRef}
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
      }
    };

    fetchDataOnMount();
  }, []);

  useEffect(() => {
    if (reclassifiedDocumentUpdate) {
      closeReclassify();
    }
  }, [reclassifiedDocumentUpdate, closeReclassify, documentId]);

  if (loading) {
    return <div>loading data</div>;
  }
  return (
    <div className={classes.reClassifyStages}>
      <LinkButton
        className={classes.backBtn}
        onClick={handleBackBtnClick}
        disabled={
          state.reClassifySaveStatus === "saving" ||
          state.reClassifySaveStatus === "success"
        }
      >
        Back
      </LinkButton>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
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
            <ReclassifyStage3 presentationTitle={presentationTitle} />
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
