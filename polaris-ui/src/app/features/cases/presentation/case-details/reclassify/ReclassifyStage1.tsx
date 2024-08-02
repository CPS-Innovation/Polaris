import { useMemo } from "react";

import { Select } from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";

type ReclassifyStage1Props = {
  presentationTitle: string;
};

export const ReclassifyStage1: React.FC<ReclassifyStage1Props> = ({
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext()!;

  const { state, dispatch } = reclassifyContext;
  const docTypes = useMemo(() => {
    const defaultValue = {
      value: "",
      children: "Choose document type",
      disabled: true,
    };
    const mappedValues = state.materialTypeList.map(
      ({ code, description }) => ({
        value: code,
        children: description,
      })
    );
    return [defaultValue, ...mappedValues];
  }, [state.materialTypeList]);

  const handleDocTypeChange = (value: string) => {
    console.log("value>>>", value);
    dispatch({ type: "UPDATE_DOCUMENT_TYPE", payload: { id: value } });
  };
  return (
    <div>
      <h1>Select the document type</h1>
      <Select
        // errorMessage={
        //   {
        //     children: "Select a different document",
        //   }
        // }
        label={{
          htmlFor: "select-reclassify-document-type",
          children: (
            <span>
              Select the document type for <b>{presentationTitle}</b>
            </span>
          ),
        }}
        id="select-reclassify-document-type"
        data-testid="select-reclassify-document-type"
        items={docTypes}
        value={state.newDocTypeId}
        onChange={(ev) => handleDocTypeChange(ev.target.value)}
      />
    </div>
  );
};
