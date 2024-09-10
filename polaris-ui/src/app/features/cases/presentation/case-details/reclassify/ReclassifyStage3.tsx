import { useMemo } from "react";
import {
  Table,
  LinkButton,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";
import classes from "./Reclassify.module.scss";

type ReclassifyStage3Props = {
  presentationTitle: string;
};

export const ReclassifyStage3: React.FC<ReclassifyStage3Props> = ({
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext()!;

  const { state, dispatch } = reclassifyContext;

  const getMaterialType = (id: string) => {
    if (!state.newDocTypeId) {
      return "";
    }
    return (
      state.materialTypeList.find((type) => type.typeId === +id)?.description ??
      ""
    );
  };
  const handleChangeBtnClick = () => {
    dispatch({
      type: "UPDATE_CLASSIFY_STAGE",
      payload: { newStage: "stage2" },
    });
  };

  const documentFieldNames = useMemo(
    () => ({
      Immediate: ["Type", "Name"],
      Other: ["Type", "Name", "Status"],
      Statement: [
        "Type",
        "Statement Witness",
        "Statement Date",
        "Statement Number",
        "Status",
      ],
      Exhibit: [
        "Type",
        "Item Name",
        "Exhibit Reference",
        "Exhibit Producer",
        "Status",
      ],
    }),
    []
  );

  const activeFieldNames = useMemo(() => {
    return documentFieldNames[`${state.reclassifyVariant}`];
  }, [documentFieldNames, state.reclassifyVariant]);

  const getFieldValue = (fieldName: string) => {
    switch (fieldName) {
      case "Type":
        return getMaterialType(state.newDocTypeId);
      case "Name":
        return state.formData.documentRenameStatus === "YES"
          ? state.formData.documentNewName
          : presentationTitle;
      case "Status":
        return state.formData.documentUsedStatus === "YES" ? "Used" : "Unused";
      case "Statement Witness": {
        if (!state.formData.statementWitnessId) return "";
        return state.statementWitness!.find(
          (witness) => witness.id === +state.formData.statementWitnessId
        )?.name;
      }
      case "Statement Date":
        return state.formData.statementDay &&
          state.formData.statementMonth &&
          state.formData.statementYear
          ? `${state.formData.statementDay}/${state.formData.statementMonth}/${state.formData.statementYear}`
          : "";
      case "Statement Number":
        return state.formData.statementNumber;
      case "Item Name":
        return state.formData.exhibitItemName;
      case "Exhibit Reference":
        return state.formData.exhibitReference;
      case "Exhibit Producer":
        if (state.formData.exhibitProducerId === "other")
          return state.formData.exhibitOtherProducerValue;
        return state.exhibitProducers!.find(
          (producer) => producer.id === +state.formData.exhibitProducerId
        )?.exhibitProducer;
    }
  };

  const getTableRows = () => {
    return activeFieldNames.map((fieldName) => ({
      cells: [
        { children: <span>{fieldName}</span> },
        { children: <span>{getFieldValue(fieldName)}</span> },
        {
          children: (
            <LinkButton
              className={classes.btnChange}
              onClick={handleChangeBtnClick}
              disabled={
                state.reClassifySaveStatus === "saving" ||
                state.reClassifySaveStatus === "success"
              }
            >
              Change
            </LinkButton>
          ),
        },
      ],
    }));
  };
  return (
    <div>
      <h1>Check your answers</h1>
      <h2>Document details</h2>
      <Table rows={getTableRows()} />
    </div>
  );
};
