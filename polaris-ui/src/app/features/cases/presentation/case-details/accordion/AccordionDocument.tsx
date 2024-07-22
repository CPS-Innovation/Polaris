import { useRef, useEffect, useCallback, useMemo } from "react";
import {
  CommonDateTimeFormats,
  formatDate,
  formatTime,
} from "../../../../../common/utils/dates";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { MappedCaseDocument } from "../../../domain/MappedCaseDocument";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { ReactComponent as DateIcon } from "../../../../../common/presentation/svgs/date.svg";
import { ReactComponent as TimeIcon } from "../../../../../common/presentation/svgs/time.svg";
import { ReactComponent as AttachmentIcon } from "../../../../../common/presentation/svgs/attachment.svg";
import { ReactComponent as NotesIcon } from "../../../../../common/presentation/svgs/notesIcon.svg";
import { ReactComponent as MoreIcon } from "../../../../../common/presentation/svgs/more.svg";

import classes from "./Accordion.module.scss";
import {
  witnessIndicatorNames,
  witnessIndicatorPrecedenceOrder,
} from "../../../domain/WitnessIndicators";
import { Tooltip } from "../../../../../common/presentation/components";
import { NotesData } from "../../../domain/gateway/NotesData";
import { mapConversionStatusToMessage } from "../../../domain/gateway/PipelineDocument";
import {
  DropdownButton,
  DropdownButtonItem,
} from "../../../../../common/presentation/components/DropdownButton";

type Props = {
  activeDocumentId: string;
  lastFocusDocumentId: string;
  readUnreadData: string[];
  caseDocument: MappedCaseDocument;
  featureFlags: {
    notes: boolean;
    renameDocument: boolean;
  };
  showDocumentRenameFeature: boolean;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
  handleOpenPanel: (
    documentId: string,
    documentCategory: string,
    presentationFileName: string,
    type: "notes" | "rename",
    documentType: string
  ) => void;
  handleGetNotes: (documentId: string) => void;
  notesData: NotesData[];
};

export const AccordionDocument: React.FC<Props> = ({
  lastFocusDocumentId,
  activeDocumentId,
  readUnreadData,
  caseDocument,
  featureFlags,
  showDocumentRenameFeature,
  notesData,
  handleOpenPdf,
  handleOpenPanel,
  handleGetNotes,
}) => {
  const openNotesBtnRef = useRef<HTMLButtonElement | null>(null);
  const trackEvent = useAppInsightsTrackEvent();

  useEffect(() => {
    if (openNotesBtnRef.current) {
      openNotesBtnRef.current.focus();
    }
  }, []);

  const canViewDocument =
    caseDocument.presentationFlags?.read === "Ok" &&
    caseDocument.conversionStatus === "DocumentConverted";
  const getAttachmentText = () => {
    if (caseDocument.attachments.length === 1) {
      return "1 attachment";
    }
    return `${caseDocument.attachments.length} attachments`;
  };

  const formattedFileCreatedTime = formatTime(caseDocument.cmsFileCreatedDate);

  const openNotesRefProps =
    caseDocument.documentId === lastFocusDocumentId
      ? { ref: openNotesBtnRef }
      : {};

  const isNotesDisabled = useCallback(() => {
    if (
      caseDocument.cmsDocType.documentType === "PCD" ||
      caseDocument.cmsDocType.documentCategory === "Review"
    ) {
      return true;
    }
    return false;
  }, [
    caseDocument.cmsDocType.documentType,
    caseDocument.cmsDocType.documentCategory,
  ]);

  const openNotesBtnAriaLabel = useCallback(() => {
    if (isNotesDisabled()) {
      return `Notes are disabled for this document`;
    }
    return caseDocument.hasNotes
      ? `There are notes available for document ${caseDocument.presentationFileName}, Open notes`
      : `There are no notes available for document ${caseDocument.presentationFileName}, Open notes`;
  }, [
    caseDocument.hasNotes,
    caseDocument.presentationFileName,
    isNotesDisabled,
  ]);

  const notesHoverOverCallback = () => {
    const documentNote = notesData.find(
      (note) => note.documentId === caseDocument.documentId
    );
    if (documentNote?.getNoteStatus !== "failure") {
      handleGetNotes(caseDocument.documentId);
    }
  };

  const getNotesHoverOverText = (ariaLiveText: boolean) => {
    if (isNotesDisabled()) return "Notes are disabled for this document";
    if (!caseDocument.hasNotes) return "";
    const documentNote = notesData.find(
      (note) => note.documentId === caseDocument.documentId
    );
    const notes = documentNote?.notes ?? [];
    if (documentNote?.getNoteStatus === "failure")
      return "Failed to retrieve notes";
    if (notes) if (!notes.length) return "Loading notes, please wait...";
    if (notes.length === 1) {
      return ariaLiveText
        ? `recent note text is ${notes[notes.length - 1].text}`
        : `${notes[notes.length - 1].text}`;
    }
    return ariaLiveText
      ? `recent note text is ${notes[notes.length - 1].text}, and ${
          notes.length - 1
        } more`
      : `${notes[notes.length - 1].text} (+${notes.length - 1} more)`;
  };

  const dropDownItems = useMemo(() => {
    let items: DropdownButtonItem[] = [];
    if (!featureFlags.renameDocument) return items;
    if (
      showDocumentRenameFeature &&
      caseDocument.presentationFlags.renameStatus === "Ok"
    ) {
      items = [
        {
          id: "1",
          label: "Rename document",
          ariaLabel: "Rename document",
          disabled: false,
        },
        ...items,
      ];
    }

    return items;
  }, [
    showDocumentRenameFeature,
    caseDocument.presentationFlags.renameStatus,
    featureFlags.renameDocument,
  ]);

  const handleDocumentAction = (id: string) => {
    switch (id) {
      case "1":
        handleOpenPanel(
          caseDocument.documentId,
          caseDocument.cmsDocType.documentCategory,
          caseDocument.presentationFileName,
          "rename",
          caseDocument.cmsDocType.documentType
        );

        break;
      default:
        break;
    }
  };

  return (
    <li
      className={`${classes["accordion-document-list-item"]} ${
        readUnreadData.includes(caseDocument.documentId) ? classes.docRead : ""
      } ${
        activeDocumentId === caseDocument.documentId ? classes.docActive : ""
      }`}
      data-read={`${
        readUnreadData.includes(caseDocument.documentId) ? "true" : "false"
      }`}
    >
      <div className={classes.listItemWrapper}>
        <div className={`${classes["accordion-document-item-wrapper"]}`}>
          <div>
            {activeDocumentId === caseDocument.documentId && (
              <strong
                className={`govuk-tag govuk-tag--turquoise ${classes.tag}`}
              >
                Active Document
              </strong>
            )}
            {canViewDocument ? (
              <LinkButton
                onClick={() => {
                  trackEvent("Open Document From Case Details", {
                    documentId: caseDocument.documentId,
                  });
                  handleOpenPdf({ documentId: caseDocument.documentId });
                }}
                className={`${classes["accordion-document-link-button"]}`}
                dataTestId={`link-document-${caseDocument.documentId}`}
                ariaLabel={`Open Document ${caseDocument.presentationFileName}`}
              >
                {caseDocument.presentationFileName}
              </LinkButton>
            ) : (
              <span
                className={`${classes["accordion-document-link-name"]}`}
                data-testid={`name-text-document-${caseDocument.documentId}`}
              >
                {caseDocument.presentationFileName}
              </span>
            )}
            <div className={`${classes["accordion-information-items"]}`}>
              {caseDocument.cmsFileCreatedDate && (
                <div className={`${classes["accordion-document-date"]}`}>
                  <span className={`${classes["visuallyHidden"]}`}>
                    {" "}
                    Date Added
                  </span>
                  <DateIcon className={classes.dateIcon} />
                  <span>
                    {formatDate(
                      caseDocument.cmsFileCreatedDate,
                      CommonDateTimeFormats.ShortDateTextMonth
                    )}
                  </span>
                </div>
              )}
              {formattedFileCreatedTime && (
                <>
                  <span className={`${classes["visuallyHidden"]}`}>
                    Time added
                  </span>
                  <TimeIcon className={classes.timeIcon} />
                  {caseDocument.cmsFileCreatedDate && formattedFileCreatedTime}
                </>
              )}
              {featureFlags.notes && (
                <Tooltip
                  text={getNotesHoverOverText(false)}
                  className="notesToolTip"
                  onHoverCallback={notesHoverOverCallback}
                >
                  {caseDocument.hasNotes && (
                    <div
                      data-testid={`recent-notes-live-text-${caseDocument.documentId}`}
                      role="status"
                      aria-live="polite"
                      className={classes.visuallyHidden}
                    >
                      {getNotesHoverOverText(true)}
                    </div>
                  )}
                  <LinkButton
                    {...openNotesRefProps}
                    className={classes.notesBtn}
                    id={`btn-notes-${caseDocument.documentId}`}
                    dataTestId={`btn-notes-${caseDocument.documentId}`}
                    ariaLabel={openNotesBtnAriaLabel()}
                    onClick={() => {
                      trackEvent("Open Notes", {
                        documentId: caseDocument.documentId,
                        documentCategory:
                          caseDocument.cmsDocType.documentCategory,
                      });
                      handleOpenPanel(
                        caseDocument.documentId,
                        caseDocument.cmsDocType.documentCategory,
                        caseDocument.presentationFileName,
                        "notes",
                        caseDocument.cmsDocType.documentType
                      );
                    }}
                    onFocus={
                      caseDocument.hasNotes ? notesHoverOverCallback : undefined
                    }
                    disabled={isNotesDisabled()}
                    aria-disabled={isNotesDisabled() ? "true" : "false"}
                  >
                    <NotesIcon />
                    {caseDocument.hasNotes && (
                      <div
                        data-testid={`has-note-indicator-${caseDocument.documentId}`}
                        className={classes.notesAvailable}
                      ></div>
                    )}
                  </LinkButton>
                </Tooltip>
              )}
            </div>

            {!!caseDocument.attachments.length && (
              <div className={classes.attachmentWrapper}>
                <AttachmentIcon className={classes.attachmentIcon} />
                <span
                  data-testid={`attachment-text-${caseDocument.documentId}`}
                >
                  {getAttachmentText()}
                </span>
              </div>
            )}
          </div>

          {!!dropDownItems.length && (
            <DropdownButton
              name=""
              dropDownItems={dropDownItems}
              callBackFn={handleDocumentAction}
              ariaLabel="document actions dropdown"
              dataTestId={`document-actions-dropdown-${caseDocument.documentId}`}
              showLastItemSeparator={true}
              icon={<MoreIcon />}
            />
          )}
        </div>
        <div className={classes.witnessIndicators}>
          {caseDocument.witnessIndicators.length > 0 &&
            caseDocument.witnessIndicators
              .sort(
                (a, b) =>
                  witnessIndicatorPrecedenceOrder.indexOf(a) -
                  witnessIndicatorPrecedenceOrder.indexOf(b)
              )
              .map((indicator) => (
                <strong
                  className={`govuk-tag govuk-tag--grey ${classes.tooltip}`}
                  key={indicator}
                  data-testid={`indicator-${caseDocument.documentId}-${indicator}`}
                >
                  {indicator}{" "}
                  <span className={classes.tooltiptext}>
                    {witnessIndicatorNames[indicator]}
                  </span>
                </strong>
              ))}
        </div>

        {!canViewDocument && (
          <span
            className={`${classes["accordion-document-read-warning"]}`}
            data-testid={`view-warning-document-${caseDocument.documentId}`}
          >
            Document only available on CMS
            {caseDocument.conversionStatus !== "DocumentConverted"
              ? `: ${mapConversionStatusToMessage(
                  caseDocument.conversionStatus
                )}`
              : ""}
          </span>
        )}
        {caseDocument.hasFailedAttachments && (
          <div className={classes.attachmentWrapper}>
            <AttachmentIcon className={classes.attachmentIcon} />
            <span
              className={`${classes["failed-attachment-warning"]}`}
              data-testid={`failed-attachment-warning-${caseDocument.documentId}`}
            >
              Attachments only available on CMS
            </span>
          </div>
        )}
      </div>
    </li>
  );
};
