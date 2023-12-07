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
  savingStatus: SavingStatus;
  redactionTypes: RedactionTypes[];
  redactionLogData: RedactionLogData;
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
  redactionTypes,
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
        redactionTypes={redactionTypes}
        savingStatus={savingStatus}
        redactionLogData={redactionLogData}
        saveRedactionLog={saveRedactionLog}
      />
    </Modal>
  );
};
