import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyStages } from "./ReclassifyStages";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { StatementWitnessNumber } from "./data/StatementWitnessNumber";
import { ReclassifySaveData } from "./data/ReclassifySaveData";

type ReclassifyProps = {
  documentId: string;
  presentationTitle: string;
  reclassifiedDocumentUpdate?: boolean;
  handleCancelReclassify: () => void;
  getMaterialTypeList: () => Promise<MaterialType[]>;
  getExhibitProducers: () => Promise<ExhibitProducer[]>;
  getStatementWitnessDetails: () => Promise<StatementWitness[]>;
  getWitnessStatementNumbers: (
    witnessId: number
  ) => Promise<StatementWitnessNumber[]>;
  handleSubmitReclassify: (
    documentId: string,
    data: ReclassifySaveData
  ) => Promise<boolean>;
};

export const Reclassify: React.FC<ReclassifyProps> = ({
  documentId,
  presentationTitle,
  reclassifiedDocumentUpdate,
  handleCancelReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  handleSubmitReclassify,
}) => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyStages
          documentId={documentId}
          presentationTitle={presentationTitle}
          reclassifiedDocumentUpdate={reclassifiedDocumentUpdate}
          handleCancelReclassify={handleCancelReclassify}
          getMaterialTypeList={getMaterialTypeList}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
          getWitnessStatementNumbers={getWitnessStatementNumbers}
          handleSubmitReclassify={handleSubmitReclassify}
        />
      </ReClassifyProvider>
    </div>
  );
};
