import { ReClassifyProvider } from "./context/ReClassifyProvider";
import { ReclassifyStages } from "./ReclassifyStages";
import { MaterialType } from "./data/MaterialType";
import { ExhibitProducer } from "./data/ExhibitProducer";
import { StatementWitness } from "./data/StatementWitness";
import { StatementWitnessNumber } from "./data/StatementWitnessNumber";
import { ReclassifySaveData } from "./data/ReclassifySaveData";

type ReclassifyProps = {
  documentId: string;
  documentVersionId: number | null;
  currentDocTypeId: number | null;
  presentationTitle: string;
  reclassifiedDocumentUpdate?: boolean;
  handleCloseReclassify: (documentId: string) => void;
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
  handleReclassifyTracking: (
    name: string,
    properties: Record<string, any>
  ) => void;
  getReclassifyThumbnail: (
    documentId: string,
    documentVersionId: number
  ) => Promise<string | null>;
};

export const Reclassify: React.FC<ReclassifyProps> = ({
  documentId,
  documentVersionId,
  currentDocTypeId,
  presentationTitle,
  reclassifiedDocumentUpdate,
  handleCloseReclassify,
  getMaterialTypeList,
  getExhibitProducers,
  getStatementWitnessDetails,
  getWitnessStatementNumbers,
  handleSubmitReclassify,
  handleReclassifyTracking,
  getReclassifyThumbnail,
}) => {
  return (
    <div>
      <ReClassifyProvider>
        <ReclassifyStages
          documentId={documentId}
          documentVersionId={documentVersionId}
          currentDocTypeId={currentDocTypeId}
          presentationTitle={presentationTitle}
          reclassifiedDocumentUpdate={reclassifiedDocumentUpdate}
          handleCloseReclassify={handleCloseReclassify}
          getMaterialTypeList={getMaterialTypeList}
          getExhibitProducers={getExhibitProducers}
          getStatementWitnessDetails={getStatementWitnessDetails}
          getWitnessStatementNumbers={getWitnessStatementNumbers}
          handleSubmitReclassify={handleSubmitReclassify}
          handleReclassifyTracking={handleReclassifyTracking}
          getReclassifyThumbnail={getReclassifyThumbnail}
        />
      </ReClassifyProvider>
    </div>
  );
};
