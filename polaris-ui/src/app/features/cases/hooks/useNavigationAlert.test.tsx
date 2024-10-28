import { renderHook } from "@testing-library/react-hooks";
import { CaseDocumentViewModel } from "../domain/CaseDocumentViewModel";
import { useNavigationAlert } from "./useNavigationAlert";
import { createMemoryHistory } from "history";
import { Router } from "react-router-dom";
import { ConversionStatus } from "../domain/gateway/PipelineDocument";

describe("useNavigationAlert hook", () => {
  const history = createMemoryHistory();
  const tabItems: CaseDocumentViewModel[] = [
    {
      clientLockedState: "unlocked",

      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      cmsFileCreatedDate: "2020-06-02",
      documentId: "1",
      cmsOriginalFileName: "MCLOVEMG3  very long .pdf",
      presentationTitle: "MCLOVEMG3  very long",
      mode: "read",
      presentationCategory: "Reviews",
      presentationSubCategory: null,
      versionId: 1,
      categoryListOrder: null,
      attachments: [],
      parentDocumentId: null,
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      redactionHighlights: [
        {
          id: "1",
          type: "redaction",
          position: {} as any,
          highlightType: "area",
          redactionType: { id: "1", name: "Address" },
        },
      ],
      pageDeleteRedactions: [],
      pageRotations: [],
      rotatePageMode: false,
      sasUrl: undefined,
      url: undefined,
      isDeleted: false,
      saveStatus: { type: "none", status: "initial" },
      witnessId: null,
      witnessIndicators: [],
      hasFailedAttachments: false,
      areaOnlyRedactionMode: false,
      hasNotes: false,
      conversionStatus: "DocumentConverted",
      isUnused: false,
      isInbox: false,
      isOcrProcessed: false,
      classification: null,
      isWitnessManagement: false,
      canReclassify: false,
      canRename: false,
      renameStatus: "CanRename",
      reference: null,
      tags: [],
    },
    {
      clientLockedState: "unlocked",
      cmsDocType: {
        documentTypeId: 3,
        documentType: "MG3",
        documentCategory: "MGForm",
      },
      cmsFileCreatedDate: "2020-06-02",
      documentId: "2",
      cmsOriginalFileName: "MCLOVEMG4 very long .pdf",
      presentationTitle: "MCLOVEMG4 test",
      mode: "read",
      presentationCategory: "Reviews",
      presentationSubCategory: null,
      versionId: 2,
      categoryListOrder: null,
      attachments: [],
      parentDocumentId: null,
      presentationFlags: {
        read: "Ok",
        write: "Ok",
      },
      redactionHighlights: [
        {
          id: "1",
          type: "redaction",
          position: {} as any,
          highlightType: "area",
          redactionType: { id: "1", name: "Address" },
        },
      ],
      pageDeleteRedactions: [],
      pageRotations: [],
      rotatePageMode: false,
      sasUrl: undefined,
      url: undefined,
      isDeleted: false,
      saveStatus: { type: "none", status: "initial" },
      witnessId: null,
      witnessIndicators: [],
      hasFailedAttachments: false,
      areaOnlyRedactionMode: false,
      hasNotes: false,
      conversionStatus: "DocumentConverted",
      isUnused: false,
      isInbox: false,
      isOcrProcessed: false,
      classification: null,
      isWitnessManagement: false,
      canReclassify: false,
      canRename: false,
      renameStatus: "CanRename",
      reference: null,
      tags: [],
    },
  ];

  it("should return the correct unSavedRedactionDocs array", () => {
    const { result } = renderHook(() => useNavigationAlert(tabItems), {
      wrapper: ({ children }) => (
        <>
          <Router history={history}>
            <div>{children}</div>
          </Router>
        </>
      ),
    });
    expect(result.current.unSavedRedactionDocs).toEqual([
      {
        documentId: "1",
        presentationTitle: "MCLOVEMG3  very long",
      },
      {
        documentId: "2",
        presentationTitle: "MCLOVEMG4 test",
      },
    ]);
  });

  // it("Should return empty array if the there are no active redactions in any document items", () => {
  //   const { result: resultOne } = renderHook(() => useNavigationAlert([]), {
  //     wrapper: ({ children }) => (
  //       <>
  //         <Router history={history}>
  //           <div>{children}</div>
  //         </Router>
  //       </>
  //     ),
  //   });
  //   expect(resultOne.current.unSavedRedactionDocs).toEqual([]);
  // });
});
