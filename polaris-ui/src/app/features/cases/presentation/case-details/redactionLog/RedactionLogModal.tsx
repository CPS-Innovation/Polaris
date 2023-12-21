import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  RedactionLogMappingData,
  RedactionLogLookUpsData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";

type Props = {
  caseUrn: string;
  isCaseCharged: boolean;
  documentName: string;
  cmsDocumentTypeId: number;
  saveStatus: SaveStatus;
  savedRedactionTypes: RedactionTypeData[];
  redactionLogLookUpsData: RedactionLogLookUpsData;
  redactionLogMappingsData: RedactionLogMappingData | null;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  caseUrn,
  isCaseCharged,
  documentName,
  cmsDocumentTypeId,
  savedRedactionTypes,
  saveStatus,
  redactionLogLookUpsData,
  redactionLogMappingsData,
  saveRedactionLog,
}) => {
  return (
    <Modal
      isVisible={true}
      type="data"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <RedactionLogContent
        caseUrn={caseUrn}
        isCaseCharged={isCaseCharged}
        documentName={documentName}
        cmsDocumentTypeId={cmsDocumentTypeId}
        savedRedactionTypes={savedRedactionTypes}
        saveStatus={saveStatus}
        redactionLogLookUpsData={redactionLogLookUpsData}
        saveRedactionLog={saveRedactionLog}
        redactionLogMappingsData={redactionLogMappingsData}
      />
    </Modal>
  );
};
