import { MaterialType, ReclassifyVariant } from "../data/MaterialType";
import { ExhibitProducer } from "../data/ExhibitProducer";
import { StatementWitness } from "../data/StatementWitness";

export type ReclassifyState = {
  materialTypeList: MaterialType[];
  exhibitProducers: ExhibitProducer[];
  statementWitness: StatementWitness[];
  currentDocTypeId: string;
  newDocTypeId: string;
  reclassifyVariant: ReclassifyVariant;

  reClassifyStage: "stage1" | "stage2" | "stage3";
};

export type ReclassifyActions =
  | {
      type: "ADD_MATERIAL_TYPE_LIST";
      payload: { materialList: MaterialType[] };
    }
  | {
      type: "ADD_EXHIBIT_PRODUCERS";
      payload: { exhibitProducers: ExhibitProducer[] };
    }
  | {
      type: "ADD_STATEMENT_WITNESSS";
      payload: { statementWitness: StatementWitness[] };
    }
  | {
      type: "UPDATE_DOCUMENT_TYPE";
      payload: { id: string };
    }
  | {
      type: "UPDATE_CLASSIFY_STAGE";
      payload: { newStage: "stage1" | "stage2" | "stage3" };
    };
export const reclassifyInitialState: ReclassifyState = {
  materialTypeList: [],
  exhibitProducers: [],
  statementWitness: [],
  currentDocTypeId: "",
  newDocTypeId: "",
  reclassifyVariant: "IMMEDIATE",
  reClassifyStage: "stage1",
};

const getReclassifyVariant = (
  materialTypeList: MaterialType[],
  code: string
) => {
  const selectedType = materialTypeList.find((type) => type.code === code)!;
  if (selectedType.classification === "STATEMENT") {
    return "STATEMENT";
  } else if (selectedType.classification === "EXHIBIT") {
    return "EXHIBIT";
  } else if (selectedType.addAsUsedOrUnused === "Y") {
    return "OTHER";
  } else {
    return "IMMEDIATE";
  }
};

export const reClassifyReducer = (
  state: ReclassifyState,
  action: ReclassifyActions
): ReclassifyState => {
  switch (action.type) {
    case "ADD_MATERIAL_TYPE_LIST": {
      return {
        ...state,
        materialTypeList: action.payload.materialList,
      };
    }
    case "ADD_EXHIBIT_PRODUCERS": {
      return {
        ...state,
        exhibitProducers: action.payload.exhibitProducers,
      };
    }
    case "ADD_STATEMENT_WITNESSS": {
      return {
        ...state,
        statementWitness: action.payload.statementWitness,
      };
    }
    case "UPDATE_DOCUMENT_TYPE": {
      return {
        ...state,
        newDocTypeId: action.payload.id,
        reclassifyVariant: getReclassifyVariant(
          state.materialTypeList,
          action.payload.id
        ),
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
