import { useState } from "react";
import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";

type Props = {
  redactionHighlights: IPdfHighlight[];
  handleShowHideDocumentIssueModal?: (value: boolean) => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  handleShowHideDocumentIssueModal,
  redactionHighlights,
}) => {
  return (
    <Modal
      isVisible={true}
      type="data"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <RedactionLogContent redactionHighlights={redactionHighlights} />
    </Modal>
  );
};
