export type ReclassifyState = {
  currentDocTypeId: string;
  newDocTypeId: string;
  reclassifyType: "type1" | "type2" | "type3" | "type4" | "initial";
  reClassifyStage: "stage1" | "stage2" | "stage3";
};

export type ReclassifyActions =
  | {
      type: "UPDATE_DOCUMENT_TYPE";
      payload: { id: string };
    }
  | {
      type: "UPDATE_CLASSIFY_STAGE";
      payload: { newStage: "stage1" | "stage2" | "stage3" };
    };
export const reclassifyInitialState: ReclassifyState = {
  currentDocTypeId: "MG2",
  newDocTypeId: "",
  reclassifyType: "initial",
  reClassifyStage: "stage1",
};

const getReclassifyType = (id: string) => {
  switch (id) {
    case "MG1":
      return "type1" as const;
    case "MG2":
      return "type2" as const;
    case "MG3":
      return "type3" as const;
    default:
      return "type4" as const;
  }
};

export const reClassifyReducer = (
  state: ReclassifyState,
  action: ReclassifyActions
): ReclassifyState => {
  switch (action.type) {
    case "UPDATE_DOCUMENT_TYPE": {
      return {
        ...state,
        newDocTypeId: action.payload.id,
        reclassifyType: getReclassifyType(action.payload.id),
      };
    }
    case "UPDATE_CLASSIFY_STAGE": {
      return {
        ...state,
        reClassifyStage: action.payload.newStage,
      };
    }
    default:
      return state;
  }
};
