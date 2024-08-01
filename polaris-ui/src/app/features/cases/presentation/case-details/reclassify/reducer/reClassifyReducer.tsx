export type ReclassifyState = {
  currentDocTypeId: string;
  newDocTypeId: string;
  reclassifyType: "type1" | "type2" | "type3" | "type4" | "initial";
};

export type ReclassifyActions = {
  type: "UPDATE_DOCUMENT_TYPE";
  payload: { id: string };
};
export const reclassifyInitialState: ReclassifyState = {
  currentDocTypeId: "MG2",
  newDocTypeId: "",
  reclassifyType: "initial",
};

const getReclassifyType = (id: string) => {
  return "type1" as const;
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
    default:
      return state;
  }
};
