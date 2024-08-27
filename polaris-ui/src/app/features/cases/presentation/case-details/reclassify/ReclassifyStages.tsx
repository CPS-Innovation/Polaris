import { useState, useEffect } from "react";
import {
  LinkButton,
  Button,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyStage1 } from "./ReclassifyStage1";
import { ReclassifyStage2 } from "./ReclassifyStage2";
import { ReclassifyStage3 } from "./ReclassifyStage3";
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

export const ReclassifyStages: React.FC<ReclassifyStagesProps> = ({
  documentId,
  presentationTitle,
  handleCancelReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  handleSubmitReclassify,
}) => {
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

  const handleContinueBtnClick = () => {
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
