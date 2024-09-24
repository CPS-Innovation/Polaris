import { MaterialType, ReclassifyVariant } from "../data/MaterialType";
import { ExhibitProducer } from "../data/ExhibitProducer";
import { StatementWitness } from "../data/StatementWitness";

export type ReclassifyState = {
  materialTypeList: MaterialType[];
  exhibitProducers: ExhibitProducer[] | null;
  statementWitness: StatementWitness[] | null;
  statementWitnessNumbers: Record<string, number[]>;
  newDocTypeId: string;
  reclassifyVariant: ReclassifyVariant;
  reClassifyStage: "stage1" | "stage2" | "stage3";
  reClassifySaveStatus: "initial" | "saving" | "success" | "failure";
  formData: {
    documentRenameStatus: "YES" | "NO";
    documentNewName: string;
    documentUsedStatus: "YES" | "NO";
    exhibitReference: string;
    exhibitItemName: string;
    exhibitProducerId: string;
    exhibitOtherProducerValue: string;
    statementWitnessId: string;
    statementDay: string;
    statementMonth: string;
    statementYear: string;
    statementNumber: string;
  };
};

export const reclassifyInitialState: ReclassifyState = {
  materialTypeList: [],
  exhibitProducers: null,
  statementWitness: null,
  statementWitnessNumbers: {},
  newDocTypeId: "",
  reclassifyVariant: "Immediate",
  reClassifyStage: "stage1",
  reClassifySaveStatus: "initial",
  formData: {
    documentRenameStatus: "NO",
    documentNewName: "",
    documentUsedStatus: "YES",
    exhibitReference: "",
    exhibitItemName: "",
    exhibitProducerId: "",
    exhibitOtherProducerValue: "",
    statementWitnessId: "",
    statementDay: "",
    statementMonth: "",
    statementYear: "",
    statementNumber: "",
  },
};

export type ReclassifyActions =
  | {
      type: "RESET_FORM_DATA";
      payload: { presentationTitle: string };
    }
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
    }
  | {
      type: "UPDATE_DOCUMENT_RENAME_STATUS";
      payload: { value: "YES" | "NO" };
    }
  | {
      type: "UPDATE_DOCUMENT_NEW_NAME";
      payload: { newName: string };
    }
  | {
      type: "UPDATE_DOCUMENT_USED_STATUS";
      payload: { value: "YES" | "NO" };
    }
  | {
      type: "UPDATE_EXHIBIT_ITEM_REFERENCE";
      payload: { value: string };
    }
  | {
      type: "UPDATE_EXHIBIT_ITEM_NAME";
      payload: { value: string };
    }
  | {
      type: "UPDATE_EXHIBIT_PRODUCER_ID";
      payload: { value: string };
    }
  | {
      type: "UPDATE_EXHIBIT_OTHER_PRODUCER_VALUE";
      payload: { value: string };
    }
  | {
      type: "UPDATE_STATEMENT_WITNESS_ID";
      payload: { value: string };
    }
  | {
      type: "UPDATE_STATEMENT_DATE";
      payload: { type: "day" | "month" | "year"; value: string };
    }
  | {
      type: "UPDATE_STATEMENT_NUMBER";
      payload: { value: string };
    }
  | {
      type: "UPDATE_RECLASSIFY_SAVE_STATUS";
      payload: { value: "initial" | "saving" | "success" | "failure" };
    }
  | {
      type: "UPDATE_STATEMENT_WITNESS_NUMBERS";
      payload: { witnessId: number; statementNumbers: number[] };
    };

const getReclassifyVariant = (
  materialTypeList: MaterialType[],
  code: string
) => {
  const selectedType = materialTypeList.find((type) => type.typeId === +code)!;

  if (selectedType.newClassificationVariant === "Statement") {
    return "Statement";
  } else if (selectedType.newClassificationVariant === "Exhibit") {
    return "Exhibit";
  } else if (selectedType.newClassificationVariant === "Other") {
    return "Other";
  } else {
    return "Immediate";
  }
};

export const reClassifyReducer = (
  state: ReclassifyState,
  action: ReclassifyActions
): ReclassifyState => {
  switch (action.type) {
    case "RESET_FORM_DATA": {
      return {
        ...state,
        formData: {
          ...reclassifyInitialState.formData,
          exhibitItemName: action.payload.presentationTitle,
        },
      };
    }
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
    case "UPDATE_DOCUMENT_RENAME_STATUS": {
      return {
        ...state,
        formData: {
          ...state.formData,
          documentRenameStatus: action.payload.value,
        },
      };
    }
    case "UPDATE_DOCUMENT_NEW_NAME": {
      return {
        ...state,
        formData: {
          ...state.formData,
          documentNewName: action.payload.newName,
        },
      };
    }
    case "UPDATE_DOCUMENT_USED_STATUS": {
      return {
        ...state,
        formData: {
          ...state.formData,
          documentUsedStatus: action.payload.value,
        },
      };
    }
    case "UPDATE_EXHIBIT_ITEM_REFERENCE": {
      return {
        ...state,
        formData: {
          ...state.formData,
          exhibitReference: action.payload.value,
        },
      };
    }
    case "UPDATE_EXHIBIT_ITEM_NAME": {
      return {
        ...state,
        formData: {
          ...state.formData,
          exhibitItemName: action.payload.value,
        },
      };
    }

    case "UPDATE_EXHIBIT_PRODUCER_ID": {
      return {
        ...state,
        formData: {
          ...state.formData,
          exhibitProducerId: action.payload.value,
        },
      };
    }
    case "UPDATE_EXHIBIT_OTHER_PRODUCER_VALUE": {
      return {
        ...state,
        formData: {
          ...state.formData,
          exhibitOtherProducerValue: action.payload.value,
        },
      };
    }
    case "UPDATE_STATEMENT_WITNESS_ID": {
      return {
        ...state,
        formData: {
          ...state.formData,
          statementWitnessId: action.payload.value,
        },
      };
    }
    case "UPDATE_STATEMENT_WITNESS_NUMBERS": {
      return {
        ...state,
        statementWitnessNumbers: {
          ...state.statementWitnessNumbers,
          [action.payload.witnessId]: action.payload.statementNumbers,
        },
      };
    }
    case "UPDATE_STATEMENT_DATE": {
      let newFormData = state.formData;
      if (action.payload.type === "day") {
        newFormData = { ...newFormData, statementDay: action.payload.value };
      }
      if (action.payload.type === "month") {
        newFormData = { ...newFormData, statementMonth: action.payload.value };
      }
      if (action.payload.type === "year") {
        newFormData = { ...newFormData, statementYear: action.payload.value };
      }
      return {
        ...state,
        formData: newFormData,
      };
    }
    case "UPDATE_STATEMENT_NUMBER": {
      return {
        ...state,
        formData: {
          ...state.formData,
          statementNumber: action.payload.value,
        },
      };
    }
    case "UPDATE_RECLASSIFY_SAVE_STATUS": {
      return {
        ...state,
        reClassifySaveStatus: action.payload.value,
      };
    }
    default:
      return state;
  }
};
