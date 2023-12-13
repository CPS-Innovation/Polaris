import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  RedactionLogData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";

type Props = {
  caseUrn: string;
  documentName: string;
  saveStatus: SaveStatus;
  savedRedactionTypes: RedactionTypeData[];
  redactionLogData: RedactionLogData;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  caseUrn,
  documentName,
  savedRedactionTypes,
  saveStatus,
  redactionLogData,
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
        redactionLogData={redactionLogData}
        saveRedactionLog={saveRedactionLog}
      />
    </Modal>
  );
};
