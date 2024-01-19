import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  RedactionLogMappingData,
  RedactionLogLookUpsData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { useCallback } from "react";

type Props = {
  caseUrn: string;
  isCaseCharged: boolean;
  owningUnit: string;
  documentName: string;
  cmsDocumentTypeId: number;
  redactionLogType: "under" | "over";
  additionalData: {
    documentId: string;
    documentType: string;
    fileCreatedDate: string;
    originalFileName: string;
  };
  saveStatus: SaveStatus;
  savedRedactionTypes: RedactionTypeData[];
  redactionLogLookUpsData: RedactionLogLookUpsData;
  redactionLogMappingsData: RedactionLogMappingData | null;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
  handleHideRedactionLogModal: () => void;
};

export const RedactionLogModal: React.FC<Props> = ({
  caseUrn,
  isCaseCharged,
  owningUnit,
  documentName,
  cmsDocumentTypeId,
  redactionLogType,
  additionalData,
  savedRedactionTypes,
  saveStatus,
  redactionLogLookUpsData,
  redactionLogMappingsData,
  saveRedactionLog,
  handleHideRedactionLogModal,
}) => {
  const trackEvent = useAppInsightsTrackEvent();

  const handleCloseModal = useCallback(() => {
    trackEvent("Close Under Over Redaction Log");
    handleHideRedactionLogModal();
  }, [trackEvent, handleHideRedactionLogModal]);

  return (
    <Modal
      isVisible={true}
      handleClose={redactionLogType === "over" ? handleCloseModal : undefined}
      type="data"
      ariaLabel="Under redaction modal"
      ariaDescription="Contains form to be filled out and submitted for redaction log "
    >
      <RedactionLogContent
        redactionLogType={redactionLogType}
        caseUrn={caseUrn}
        isCaseCharged={isCaseCharged}
        owningUnit={owningUnit}
        documentName={documentName}
        cmsDocumentTypeId={cmsDocumentTypeId}
        additionalData={additionalData}
        savedRedactionTypes={savedRedactionTypes}
        saveStatus={saveStatus}
        redactionLogLookUpsData={redactionLogLookUpsData}
        saveRedactionLog={saveRedactionLog}
        redactionLogMappingsData={redactionLogMappingsData}
      />
    </Modal>
  );
};
