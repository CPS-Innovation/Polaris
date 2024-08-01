import {
  BackLink,
  Tooltip,
  LinkButton,
  PageContentWrapper,
  WaitPage,
  Select,
  Button,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";

type ReclassifyPage1Props = {};

const docTypes = [
  {
    value: "MG1",
    children: "MG1",
  },
  {
    value: "MG2",
    children: "MG2",
  },
];
export const ReclassifyPage1 = () => {
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
      <Select
        // errorMessage={
        //   {
        //     children: "Select a different document",
        //   }
        // }
        label={{
          htmlFor: "select-reclassify-document-type",
          children: "Select the document type",
          // className: classes.selectLabel,
        }}
        id="select-reclassify-document-type"
        data-testid="select-reclassify-document-type"
        items={docTypes}
        value={state.newDocTypeId}
        onChange={(ev) => handleDocTypeChange(ev.target.value)}
      />
      <div>
        <Button>Continue</Button>
        <LinkButton onClick={() => {}}>Cancel</LinkButton>
      </div>
    </div>
  );
};
