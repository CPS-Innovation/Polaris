import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyStages } from "./ReclassifyStages";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";

type ReclassifyProps = {
  presentationTitle?: string;
  handleCancelReclassify: () => void;
  getMaterialTypeList: () => Promise<MaterialType[]>;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  // handleSubmitReclassify:()=>
};

export const Reclassify: React.FC<ReclassifyProps> = ({
  handleCancelReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  presentationTitle = "Asset Rec 1",
}) => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyStages
          handleCancelReclassify={handleCancelReclassify}
          presentationTitle={presentationTitle}
          getMaterialTypeList={getMaterialTypeList}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
        />
      </ReClassifyProvider>
    </div>
  );
};
