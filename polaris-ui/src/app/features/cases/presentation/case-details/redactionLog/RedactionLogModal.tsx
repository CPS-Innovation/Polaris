import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { UnderRedactionContent } from "./UnderRedactionContent";

type Props = {
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
}) => {
  const handleConfirmationModalClose = () => {
    console.log("close");
  };
  return (
    <Modal
      isVisible={true}
      handleClose={handleConfirmationModalClose}
      type="alert"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <UnderRedactionContent />
    </Modal>
  );
};
