import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { SavingStatus } from "../../../domain/gateway/SavingStatus";
import { RedactionLogData } from "../../../domain/redactionLog/RedactionLogData";

type Props = {
  savingStatus: SavingStatus;
  redactionHighlights: IPdfHighlight[];
  redactionLogData: RedactionLogData;
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
  redactionHighlights,
  savingStatus,
  redactionLogData,
}) => {
  return (
    <Modal
      isVisible={true}
      type="data"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <RedactionLogContent
        redactionHighlights={redactionHighlights}
        savingStatus={savingStatus}
        redactionLogData={redactionLogData}
      />
    </Modal>
  );
};
