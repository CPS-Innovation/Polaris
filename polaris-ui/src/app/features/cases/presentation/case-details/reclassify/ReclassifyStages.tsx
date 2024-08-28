import { useState, useEffect } from "react";
import {
  LinkButton,
  Button,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyStage1 } from "./ReclassifyStage1";
import { ReclassifyStage2 } from "./ReclassifyStage2";
import { ReclassifyStage3 } from "./ReclassifyStage3";
import { FormDataErrors } from "./data/FormDataErrors";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { ReclassifySaveData } from "./data/ReclassifySaveData";
import classes from "./Reclassify.module.scss";

type ReclassifyStagesProps = {
  documentId: string;
  presentationTitle: string;
  handleCancelReclassify: () => void;
  getMaterialTypeList: () => Promise<MaterialType[]>;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  handleSubmitReclassify: (
    documentId: string,
    data: ReclassifySaveData
  ) => Promise<boolean>;
};

const MAX_LENGTH = 252;
export const ReclassifyStages: React.FC<ReclassifyStagesProps> = ({
  documentId,
  presentationTitle,
  handleCancelReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  handleSubmitReclassify,
}) => {
  const [formDataErrors, setFormDataErrors] = useState<FormDataErrors>({
    documentNewNameErrorText: "",
    exhibitItemNameErrorText: "",
    otherExhibitProducerErrorText: "",
    exhibitReferenceErrorText: "",
    exhibitSubjectErrorText: "",
  });

  const [loading, setLoading] = useState(false);
  const reclassifyContext = useReClassifyContext()!;

  const { state, dispatch } = reclassifyContext;
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
  }, [getMaterialTypeList, dispatch, state.materialTypeList.length]);

  const validateData = () => {
    const {
      reclassifyVariant,
      formData: {
        documentRenameStatus,
        documentNewName,
        exhibitItemName,
        exhibitReference,
        exhibitSubject,
        exhibitProducerId,
        exhibitOtherProducerValue,
      },
    } = reclassifyContext.state;

    const errorTexts: FormDataErrors = {
      documentNewNameErrorText: "",
      exhibitItemNameErrorText: "",
      otherExhibitProducerErrorText: "",
      exhibitReferenceErrorText: "",
      exhibitSubjectErrorText: "",
    };
    if (state.reClassifyStage !== "stage2") {
      setFormDataErrors(errorTexts);
      return true;
    }

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
    if (reclassifyVariant === "EXHIBIT") {
      if (!exhibitItemName) {
        errorTexts.exhibitItemNameErrorText =
          "Exhibit item name should not be empty";
      }
      if (!exhibitReference) {
        errorTexts.exhibitReferenceErrorText =
          "Exhibit reference should not be empty";
      }
      if (!exhibitSubject) {
        errorTexts.exhibitSubjectErrorText =
          "Exhibit subject should not be empty";
      }

      if (exhibitItemName.length > MAX_LENGTH) {
        errorTexts.exhibitItemNameErrorText = `Exhibit item name must be ${MAX_LENGTH} characters or less`;
      }
      if (exhibitProducerId === "other" && !exhibitOtherProducerValue) {
        errorTexts.otherExhibitProducerErrorText = `Exhibit existing producer or witness should not be empty`;
      }
    }
    setFormDataErrors(errorTexts);
    const validErrors = Object.keys(errorTexts).filter(
      (key) => errorTexts[key as keyof FormDataErrors]
    );

    console.log("validErrors>>", validErrors.length);
    return !validErrors.length;
  };

  const handleContinueBtnClick = () => {
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
      handleCancelReclassify();
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
    //get values from state.formData based on the data contract
    const saveData: ReclassifySaveData = {
      documentNewName: state.formData.documentNewName,
      documentUsedStatus: state.formData.documentUsedStatus,
    };
    dispatch({
      type: "UPDATE_RECLASSIFY_SAVE_STATUS",
      payload: { value: "saving" },
    });
    const result = await handleSubmitReclassify(documentId, saveData);
    if (result) {
      dispatch({
        type: "UPDATE_RECLASSIFY_SAVE_STATUS",
        payload: { value: "success" },
      });
      handleCancelReclassify();
      return;
    }

    dispatch({
      type: "UPDATE_RECLASSIFY_SAVE_STATUS",
      payload: { value: "failure" },
    });
    // handle the failure of saveReclassify by showing an error modal
  };
  if (loading) {
    return <div>loading data</div>;
  }
  return (
    <div className={classes.reClassifyStages}>
      <LinkButton onClick={handleBackBtnClick}>Back</LinkButton>
      {state.reClassifyStage === "stage1" && (
        <ReclassifyStage1 presentationTitle={presentationTitle} />
      )}
      {state.reClassifyStage === "stage2" && (
        <ReclassifyStage2
          presentationTitle={presentationTitle}
          formDataErrors={formDataErrors}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
        />
      )}

      {state.reClassifyStage === "stage3" && (
        <ReclassifyStage3 presentationTitle={presentationTitle} />
      )}

      <div className={classes.btnWrapper}>
        {state.reClassifyStage !== "stage3" ? (
          <>
            <Button onClick={handleContinueBtnClick}>Continue</Button>
            <LinkButton onClick={handleCancelReclassify}>Cancel</LinkButton>
          </>
        ) : (
          <>
            <Button
              onClick={handleAcceptAndSave}
              disabled={state.reClassifySaveStatus === "saving"}
            >
              Accept and save
            </Button>
          </>
        )}
      </div>
    </div>
  );
};
