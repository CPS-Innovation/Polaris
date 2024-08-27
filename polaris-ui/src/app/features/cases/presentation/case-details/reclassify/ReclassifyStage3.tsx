import { useMemo } from "react";
import {
  Table,
  LinkButton,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";

type ReclassifyStage3Props = {
  presentationTitle: string;
};

export const ReclassifyStage3: React.FC<ReclassifyStage3Props> = ({
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext()!;

  const { state, dispatch } = reclassifyContext;

  const handleChangeBtnClick = () => {
    dispatch({
      type: "UPDATE_CLASSIFY_STAGE",
      payload: { newStage: "stage2" },
    });
  };

  const documentFieldNames = useMemo(
    () => ({
      IMMEDIATE: ["Name"],
      OTHER: ["Name", "Status"],
      STATEMENT: [
        "Statement Witness",
        "Statement Date",
        "Statement Number",
        "Status",
      ],
      EXHIBIT: [
        "Name",
        "Status",
        "Exhibit Item",
        "Exhibit Reference",
        "Exhibit Item Name",
        "Exhibit Producer",
      ],
    }),
    []
  );

  const activeFieldNames = useMemo(() => {
    return documentFieldNames[`${state.reclassifyVariant}`];
  }, [documentFieldNames, state.reclassifyVariant]);

  const getFieldValue = (fieldName: string) => {
    switch (fieldName) {
      case "Name":
        return state.formData.documentRenameStatus === "YES"
          ? state.formData.documentNewName
          : presentationTitle;
      case "Status":
        return state.formData.documentUsedStatus === "YES" ? "Used" : "Unused";
      case "Statement Witness":
        return state.statementWitness.find(
          ({ witness }) => witness.id === +state.formData.statementWitnessId
        )?.witness.name;
      case "Statement Date":
        return state.formData.statementDay &&
          state.formData.statementMonth &&
          state.formData.statementYear
          ? `${state.formData.statementDay}/${state.formData.statementMonth}/${state.formData.statementYear}`
          : "";
      case "Statement Number":
        return state.formData.statementNumber;
      case "Exhibit Reference":
        return state.formData.exhibitReference;
      case "Exhibit Item Name":
        return state.formData.exhibitItemName;
      case "Exhibit Producer":
        if (state.formData.exhibitProducerId === "other")
          return state.formData.exhibitOtherProducerValue;
        return state.exhibitProducers.find(
          (producer) => producer.id === +state.formData.exhibitProducerId
        )?.fullName;
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
              onClick={handleChangeBtnClick}
              disabled={state.reClassifySaveStatus === "saving"}
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
