import { Select } from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";

type ReclassifyStage1Props = {
  presentationTitle: string;
};

const docTypes = [
  {
    value: "",
    children: "Choose document type",
    disabled: true,
  },
  {
    value: "MG1",
    children: "MG1",
  },
  {
    value: "MG2",
    children: "MG2",
  },
  {
    value: "MG3",
    children: "MG3",
  },
  {
    value: "MG4",
    children: "MG4",
  },
];
export const ReclassifyStage1: React.FC<ReclassifyStage1Props> = ({
  presentationTitle,
}) => {
  const reclassifyContext = useReClassifyContext();

  if (!reclassifyContext) {
    return <div>Context is now available</div>;
  }
  const { state, dispatch } = reclassifyContext;

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
          // className: classes.selectLabel,
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
