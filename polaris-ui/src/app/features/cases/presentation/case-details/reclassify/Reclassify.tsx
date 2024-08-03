import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyStages } from "./ReclassifyStages";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { ReclassifySaveData } from "./data/ReclassifySaveData";

type ReclassifyProps = {
  documentId: string;
  presentationTitle: string;
  handleCancelReclassify: () => void;
  getMaterialTypeList: () => Promise<MaterialType[]>;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  handleSubmitReclassify: (
    documentId: string,
    data: ReclassifySaveData
  ) => Promise<boolean>;
};

export const Reclassify: React.FC<ReclassifyProps> = ({
  documentId,
  presentationTitle,
  handleCancelReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  handleSubmitReclassify,
}) => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyStages
          documentId={documentId}
          presentationTitle={presentationTitle}
          handleCancelReclassify={handleCancelReclassify}
          getMaterialTypeList={getMaterialTypeList}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
          handleSubmitReclassify={handleSubmitReclassify}
        />
      </ReClassifyProvider>
    </div>
  );
};
