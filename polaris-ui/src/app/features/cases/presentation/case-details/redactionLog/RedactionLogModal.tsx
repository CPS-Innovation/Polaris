import { Modal } from "../../../../../common/presentation/components/index";
import { RedactionLogContent } from "./RedactionLogContent";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  RedactionLogMappingData,
  RedactionLogLookUpsData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionLogTypes } from "../../../domain/redactionLog/RedactionLogTypes";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { useCallback } from "react";
import { CmsDocType } from "../../../domain/gateway/CmsDocType";

type Props = {
  caseUrn: string;
  isCaseCharged: boolean;
  owningUnit: string;
  documentName: string;
  cmsDocumentTypeId: CmsDocType["documentTypeId"];
  redactionLogType: RedactionLogTypes;
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
  handleSaveRedactionLog: (
    data: RedactionLogRequestData,
    redactionLogType: RedactionLogTypes
  ) => void;
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
  handleSaveRedactionLog,
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
      handleClose={
        redactionLogType === RedactionLogTypes.UNDER_OVER
          ? handleCloseModal
          : undefined
      }
      type="data"
      ariaLabel={
        redactionLogType === RedactionLogTypes.UNDER_OVER
          ? "Redaction log modal"
          : "Under Redaction log modal"
      }
      ariaDescription={
        redactionLogType === RedactionLogTypes.UNDER_OVER
          ? `Fill and submit under or over redaction log form for the document ${documentName}`
          : `Fill and submit under redaction log form for the document ${documentName}`
      }
      defaultLastFocusId="#active-tab-panel"
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
        handleSaveRedactionLog={handleSaveRedactionLog}
        redactionLogMappingsData={redactionLogMappingsData}
        handleCloseRedactionLog={
          redactionLogType === RedactionLogTypes.UNDER_OVER
            ? handleCloseModal
            : undefined
        }
      />
    </Modal>
  );
};
