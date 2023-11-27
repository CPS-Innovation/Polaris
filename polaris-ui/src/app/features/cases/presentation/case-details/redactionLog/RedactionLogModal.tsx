import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";

type Props = {
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
}) => {
  return (
    <Modal
      isVisible={true}
      type="data"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <RedactionLogContent />
    </Modal>
  );
};
