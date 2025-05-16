import { useCallback, useMemo, useState, useEffect } from "react";
import { useParams } from "react-router-dom";
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
import { Tag, Tooltip } from "../../../../../common/presentation/components";
import { NotesData } from "../../../domain/gateway/NotesData";
import {
  mapConversionStatusToMessage,
  Classification,
  ConversionStatus,
  GroupedConversionStatus,
} from "../../../domain/gateway/PipelineDocument";
import {
  DropdownButton,
  DropdownButtonItem,
} from "../../../../../common/presentation/components/DropdownButton";
import { FeatureFlagData } from "../../../domain/FeatureFlagData";

type Props = {
  activeDocumentId: string;
  readUnreadData: string[];
  caseDocument: MappedCaseDocument;
  featureFlags: FeatureFlagData;
  handleOpenPdf: (caseDocument: {
    documentId: CaseDocumentViewModel["documentId"];
  }) => void;
  handleOpenPanel: (
    documentId: string,
    documentCategory: string,
    presentationTitle: string,
    type: "notes" | "rename",
    documentType: string,
    classification: Classification
  ) => void;
  handleReclassifyDocument: (documentId: string) => void;
  handleGetNotes: (documentId: string) => void;
  notesData: NotesData[];
  conversionStatus?: ConversionStatus | GroupedConversionStatus;
  handleToggleDocumentState: (
    urn: string | undefined,
    caseId: number | undefined,
    documentId: string,
    isUnsed: boolean
  ) => void;
  hkDocumentId: string | undefined;
};

export const AccordionDocument: React.FC<Props> = ({
  activeDocumentId,
  readUnreadData,
  caseDocument,
  featureFlags,
  notesData,
  conversionStatus,
  handleOpenPdf,
  handleOpenPanel,
  handleGetNotes,
  handleReclassifyDocument,
  handleToggleDocumentState,
  hkDocumentId,
}) => {
  const { id: caseId, urn } = useParams<{ id: string; urn: string }>();
  const trackEvent = useAppInsightsTrackEvent();
  const canViewDocument = conversionStatus
    ? caseDocument.presentationFlags?.read === "Ok" &&
      conversionStatus === "DocumentConverted"
    : caseDocument.presentationFlags?.read === "Ok";

  const getAttachmentText = () =>
    caseDocument.attachments.length === 1
      ? "1 attachment"
      : `${caseDocument.attachments.length} attachments`;

  const formattedFileCreatedTime = formatTime(caseDocument.cmsFileCreatedDate);

  const isNotesDisabled = useCallback(
    () =>
      caseDocument.cmsDocType.documentType === "PCD" ||
      caseDocument.cmsDocType.documentCategory === "Review",
    [
      caseDocument.cmsDocType.documentType,
      caseDocument.cmsDocType.documentCategory,
    ]
  );

  const openNotesBtnAriaLabel = useCallback(() => {
    if (isNotesDisabled()) {
      return `Notes are disabled for this document`;
    }
    return caseDocument.hasNotes
      ? `There are notes available for document ${caseDocument.presentationTitle}, Open notes`
      : `There are no notes available for document ${caseDocument.presentationTitle}, Open notes`;
  }, [caseDocument.hasNotes, caseDocument.presentationTitle, isNotesDisabled]);

  const notesHoverOverCallback = () => {
    if (isNotesDisabled()) {
      return;
    }
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
    if (
      featureFlags.renameDocument &&
      caseDocument.canRename &&
      caseDocument.presentationFlags.write !== "IsDispatched"
    ) {
      items = [
        ...items,
        {
          id: "1",
          label: "Rename document",
          ariaLabel: "Rename document",
          disabled: false,
        },
      ];
    }
    if (
      featureFlags.reclassify &&
      caseDocument.canReclassify &&
      caseDocument.presentationFlags.write !== "IsDispatched"
    ) {
      items = [
        ...items,
        {
          id: "2",
          label: "Reclassify document",
          ariaLabel: "Reclassify document",
          disabled: false,
        },
      ];
    }
    {
      console.log(featureFlags);
    }
    if (
      caseDocument.presentationFlags.write !== "IsDispatched" &&
      featureFlags.isUnused
    ) {
      const isUnused = caseDocument.isUnused ? "used" : "unused";
      items = [
        ...items,
        {
          id: `3`,
          label: `Mark as ${isUnused}`,
          ariaLabel: `Mark as ${isUnused}`,
          disabled: false,
        },
      ];
    }
    return items;
  }, [
    caseDocument.canReclassify,
    caseDocument.canRename,
    featureFlags.renameDocument,
    featureFlags.reclassify,
    caseDocument.presentationFlags.write,
    featureFlags.isUnused,
  ]);

  const handleDocumentAction = (id: string) => {
    switch (id) {
      case "1":
        handleOpenPanel(
          caseDocument.documentId,
          caseDocument.cmsDocType.documentCategory,
          caseDocument.presentationTitle,
          "rename",
          caseDocument.cmsDocType.documentType,
          caseDocument.classification
        );
        break;
      case "2":
        handleReclassifyDocument(caseDocument.documentId);
        break;
      case "3": {
        trackEvent("Update Document Evidential Status", {
          documentType: caseDocument.cmsDocType.documentTypeId,
          documentDocumentType: caseDocument.cmsDocType.documentType,
        });
        handleToggleDocumentState(
          urn,
          +caseId!,
          caseDocument.documentId,
          caseDocument.isUnused
        );

        break;
      }
      default:
    }
  };

  const listItemClasses = (
    [
      ["accordion-document-list-item", true],
      ["docRead", readUnreadData.includes(caseDocument.documentId)],
      // Not perfect, but its enough to say if the tag is green then the background
      // should be green
      ["docNew", caseDocument.tags.some((tag) => tag.color === "green")],
      ["docActive", activeDocumentId === caseDocument.documentId],
      ["docActive", hkDocumentId === caseDocument.documentId],
    ] as [string, boolean][]
  )
    .filter(([, shouldInclude]) => shouldInclude)
    .map(([className]) => classes[className]);

  useEffect(() => {
    // opens document for HouseKeeping
    // document ID is retrieved from URL
    const stringsOnlyPattern = /^[a-zA-Z]*-/;

    const isDocumentIdClean = caseDocument?.documentId?.replace(
      stringsOnlyPattern,
      ""
    );

    if (hkDocumentId === isDocumentIdClean) {
      handleOpenPdf({ documentId: caseDocument.documentId });
    }
  }, [hkDocumentId]);

  return (
    <li
      className={listItemClasses.join(" ")}
      data-read={`${readUnreadData.includes(caseDocument.documentId)}`}
      data-document-active={activeDocumentId === caseDocument.documentId}
    >
      <div className={classes.listItemWrapper}>
        {activeDocumentId === caseDocument.documentId && (
          <span>
            <Tag gdsTagColour="blue" className={classes.tag}>
              Active Document
            </Tag>{" "}
          </span>
        )}
        {caseDocument.tags.map((tag) => (
          <span key={tag.label}>
            <Tag gdsTagColour={tag.color} className={classes.tag}>
              {tag.label}
            </Tag>{" "}
          </span>
        ))}
        <div className={`${classes["accordion-document-item-wrapper"]}`}>
          <div className={`${classes.mainContentWrapper}`}>
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
                ariaLabel={`Open Document ${caseDocument.presentationTitle}`}
              >
                {caseDocument.presentationTitle}
              </LinkButton>
            ) : (
              <span
                className={`${classes["accordion-document-link-name"]}`}
                data-testid={`name-text-document-${caseDocument.documentId}`}
              >
                {caseDocument.presentationTitle}
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
                  <span className={classes.timeValue}>
                    {formattedFileCreatedTime}
                  </span>
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
                        caseDocument.presentationTitle,
                        "notes",
                        caseDocument.cmsDocType.documentType,
                        caseDocument.classification
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

            {caseDocument.reference && (
              <div>
                <span className={classes.reference}>
                  Ref:{" "}
                  <strong className={classes.referenceValue}>
                    {caseDocument.reference}
                  </strong>{" "}
                </span>
              </div>
            )}

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
              ariaLabel="document housekeeping actions dropdown"
              dataTestId={`document-housekeeping-actions-dropdown-${caseDocument.documentId}`}
              showLastItemSeparator={true}
              icon={<MoreIcon />}
            />
          )}
        </div>
        {caseDocument.witnessIndicators.length > 0 && (
          <div className={classes.witnessIndicators}>
            {caseDocument.witnessIndicators
              .sort(
                (a, b) =>
                  witnessIndicatorPrecedenceOrder.indexOf(a) -
                  witnessIndicatorPrecedenceOrder.indexOf(b)
              )
              .map((indicator) => (
                <Tag
                  gdsTagColour="grey"
                  className={classes.tooltip}
                  key={indicator}
                  data-testid={`indicator-${caseDocument.documentId}-${indicator}`}
                >
                  {indicator}{" "}
                  <span className={classes.tooltiptext}>
                    {witnessIndicatorNames[indicator]}
                  </span>
                </Tag>
              ))}
          </div>
        )}

        {!canViewDocument && (
          <span
            className={`${classes["accordion-document-read-warning"]}`}
            data-testid={`view-warning-document-${caseDocument.documentId}`}
          >
            Document only available on CMS
            {conversionStatus && conversionStatus !== "DocumentConverted"
              ? `: ${mapConversionStatusToMessage(conversionStatus)}`
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
