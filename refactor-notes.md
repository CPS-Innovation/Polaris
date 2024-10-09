# Plan

- hack model back

  - :white_check_mark: remove isPdfAvailable
  - :white_check_mark: remove cmsOriginalFileExtension, pdfBlobName, presentationFileName
  - sort out ids (numeric) and requesting for document via id, docType -> int, remove hack from gateway-api
  - e2e tests for reclassify
  - e2e test type files copied across

- read case and docs via gateway

  - conversion state stored in UI state

- how search knits in?

- rationalise reducer state (with a view to serialization)

# Immediate

## Ids

| Name                     | Current                                                     | To be                |                    |
| ------------------------ | ----------------------------------------------------------- | -------------------- | ------------------ |
| polarisDocumentId        | Exists over wire but the ignored, string prefixed "CMS-..." | Rename -> documentId |                    |
| cmsVersionId             | Numeric versionId of document                               | Rename -> versionId  |                    |
| polarisDocumentVersionId | Incrementing artificial number                              | Remove               |                    |
| cmsDocumentId            | Appears to be null all the time                             | Remove               | :white_check_mark: |

gateway-api.ts hack

```
export type PresentationDocumentProperties = {
  documentId: string;                               // add      remove hack and string
                                                    // add      versionId
  cmsDocumentId: string;                            // remove
  cmsVersionId: number;                             // remove
  cmsOriginalFileName: string;                      // keep     redaction-log
  presentationTitle: string;                        // keep
  polarisDocumentVersionId: number;                 // remove   replumb to versionId
    UPDATE_REFRESH_PIPELINE
    `api/urns/${urn}/cases/${caseId}/documents/${documentId}?v=${polarisDocumentVersionId}`
    SearchPIIData.polarisDocumentVersionId

  cmsOriginalFileExtension: string | null;          // remove and use extension from `cmsOriginalFileName` for hte
  cmsFileCreatedDate: string;                       // keep
  categoryListOrder: number | null;                 // keep
  cmsDocType: CmsDocType;                           // keep and make docTypeId an int
  presentationFlags: PresentationFlags;             // keep
  polarisParentDocumentId: string | null;           // keep     make number
  witnessId: number | null;                         // keep
  hasFailedAttachments: boolean;                    // keep
  hasNotes: boolean;                                // keep
  conversionStatus: ConversionStatus;               // THINK - local properties of document
  isUnused: boolean;                                // keep
  isInbox: boolean;                                 // keep - not used but should be useful in a sane model
  classification: Classification;                   // keep
  isWitnessManagement: boolean;                     // keep
  canReclassify: boolean;                           // keep
  canRename: boolean;                               // keep
  renameStatus:                                     // keep
    | "CanRename"
    | "IsWitnessManagement"
    | "IsDispatched"
    | "IsStatement"
    | "IsDefenceStatement";
  reference: string | null;                         // keep
};

export type PipelineDocumentProperties = {
  polarisDocumentId?: string;                       // remove
  documentId: string;                               // ok
  pdfBlobName: string;                              // remove
  isPdfAvailable?: boolean;                         // remove
  status:                                           // keep for search process
    | "New"
    | "PdfUploadedToBlob"
    | "Indexed"
    | "UnableToConvertToPdf"
    | "UnexpectedFailure"
    | "OcrAndIndexFailure";
};

export type MappedCaseDocument = PresentationDocumentProperties & {
  presentationCategory: string;
  presentationFileName: string;                     // remove
  presentationSubCategory: string | null;
  attachments: { documentId: string; name: string }[];
  witnessIndicators: WitnessIndicator[];
  tags: TagType[];
};

export type CaseDocumentViewModel = MappedCaseDocument & {
  saveStatus: SaveStatus;
  isDeleted: boolean;
  url: string | undefined;
  pdfBlobName: string | undefined;                  // remove
  sasUrl: undefined | string;                       // remove
  areaOnlyRedactionMode: boolean;
  redactionHighlights: IPdfHighlight[];
  pageDeleteRedactions: IPageDeleteRedaction[];
  clientLockedState: // note: unlocked is just the state where the client doesn't know yet
  //  (might be locked on the server, we haven't interacted yet)
  ClientLockedState;
} & (
    | { mode: "read" }
    | {
        mode: "search";
        searchTerm: string;
        occurrencesInDocumentCount: number;
        searchHighlights: IPdfHighlight[];
      }
  );
```

- lift lookups etc above case
- get rid of the CMS-12344
- get document based on version - check all of this
- UX for XLSX may be slow
- pre-emptive pdf generation from UI (pre convert the most likely/popular docs)
- get rid of PdfBlobName from backend and from mock as the mechanism for blob retrieval
- sort out CmsVersionId and PolarisDocumentId in backend/tracker
- polarisDocumentVersionId -> does this ever get employed for PCDs/DACs? How should we "store by version" for PCDs in blob storage

# Done

- do stats on pdf generation time

```
Polaris_Metrics_Conversion
| where not(IsFailure)
| summarize Count=count() by bin(DurationSeconds, 1)
| order by DurationSeconds asc
| serialize CumulativeCount=row_cumsum(Count)
| project DurationSeconds = DurationSeconds + 1, Count, CumulativeCount, Percentage = 100.0 * CumulativeCount/TotalCount
```
