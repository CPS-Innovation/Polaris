import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { SavingStatus } from "../../../domain/gateway/SavingStatus";

type Props = {
  savingStatus: SavingStatus;
  redactionHighlights: IPdfHighlight[];
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
  redactionHighlights,
  savingStatus,
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
      />
    </Modal>
  );
};
