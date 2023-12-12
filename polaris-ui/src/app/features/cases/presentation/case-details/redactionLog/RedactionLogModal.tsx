import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { SavingStatus } from "../../../domain/gateway/SavingStatus";
import {
  RedactionLogData,
  RedactionTypes,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionLogRequestData } from "../../../domain/redactionLog/ViewModal";

type Props = {
  caseUrn: string;
  documentName: string;
  savingStatus: SavingStatus;
  savedRedactionTypes: RedactionTypes[];
  redactionLogData: RedactionLogData;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  caseUrn,
  documentName,
  savedRedactionTypes,
  savingStatus,
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
        savingStatus={savingStatus}
        redactionLogData={redactionLogData}
        saveRedactionLog={saveRedactionLog}
      />
    </Modal>
  );
};
