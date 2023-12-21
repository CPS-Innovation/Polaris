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
  documentName: string;
  saveStatus: SaveStatus;
  savedRedactionTypes: RedactionTypeData[];
  redactionLogLookUpsData: RedactionLogLookUpsData;
  redactionLogMappingsData: RedactionLogMappingData | null;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  caseUrn,
  documentName,
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
        documentName={documentName}
        savedRedactionTypes={savedRedactionTypes}
        saveStatus={saveStatus}
        redactionLogLookUpsData={redactionLogLookUpsData}
        saveRedactionLog={saveRedactionLog}
        redactionLogMappingsData={redactionLogMappingsData}
      />
    </Modal>
  );
};
