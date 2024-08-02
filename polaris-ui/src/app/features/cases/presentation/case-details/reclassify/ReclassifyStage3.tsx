import {
  Table,
  LinkButton,
} from "../../../../../common/presentation/components";
import { useReClassifyContext } from "./context/ReClassifyProvider";

type ReclassifyStage3Props = {};

export const ReclassifyStage3: React.FC<ReclassifyStage3Props> = () => {
  const reclassifyContext = useReClassifyContext();

  if (!reclassifyContext) {
    return <div>Context is now available</div>;
  }
  const { state, dispatch } = reclassifyContext;

  const handleChangeBtnClick = () => {
    dispatch({
      type: "UPDATE_CLASSIFY_STAGE",
      payload: { newStage: "stage2" },
    });
  };

  const getTableRows = () => {
    return [
      {
        cells: [
          { children: <span>Type</span> },
          { children: <span>Abe</span> },
          {
            children: (
              <LinkButton onClick={handleChangeBtnClick}>Change</LinkButton>
            ),
          },
        ],
      },
      {
        cells: [
          { children: <span>Type1</span> },
          { children: <span>Abe1</span> },
          {
            children: (
              <LinkButton onClick={handleChangeBtnClick}>Change</LinkButton>
            ),
          },
        ],
      },
    ];
  };
  return (
    <div>
      <h1>Check your answers</h1>
      <h2>Document details</h2>
      <Table rows={getTableRows()} />
    </div>
  );
};
