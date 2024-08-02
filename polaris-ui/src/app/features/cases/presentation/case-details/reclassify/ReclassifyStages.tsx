import {
  LinkButton,
  Button,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import { ReclassifyStage1 } from "./ReclassifyStage1";
import { ReclassifyStage2 } from "./ReclassifyStage2";
import classes from "./Reclassify.module.scss";

type ReclassifyStagesProps = {
  presentationTitle: string;
  handleCancelReclassify: () => void;
};

export const ReclassifyStages: React.FC<ReclassifyStagesProps> = ({
  handleCancelReclassify,
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext();

  if (!reclassifyContext) {
    return <div>Context is now available</div>;
  }
  const { state, dispatch } = reclassifyContext;

  const handleContinueBtnClick = () => {
    if (state.reClassifyStage === "stage1") {
      dispatch({
        type: "UPDATE_CLASSIFY_STAGE",
        payload: { newStage: "stage2" },
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

  return (
    <div>
      <LinkButton onClick={handleBackBtnClick}>Back</LinkButton>
      {state.reClassifyStage === "stage1" && (
        <ReclassifyStage1 presentationTitle={presentationTitle} />
      )}
      {state.reClassifyStage === "stage2" && (
        <ReclassifyStage2 presentationTitle={presentationTitle} />
      )}

      <div className={classes.btnWrapper}>
        <Button onClick={handleContinueBtnClick}>Continue</Button>
        <LinkButton onClick={handleCancelReclassify}>Cancel</LinkButton>
      </div>
    </div>
  );
};
