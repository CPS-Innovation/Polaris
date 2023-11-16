import { renderHook } from "@testing-library/react-hooks";
import { CaseDocumentViewModel } from "../domain/CaseDocumentViewModel";
import { useNavigationAlert } from "./useNavigationAlert";
import { createMemoryHistory } from "history";
import { Router } from "react-router-dom";

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
      cmsDocumentId: "1",
      cmsOriginalFileName: "MCLOVEMG3  very long .docx",
      presentationTitle: "MCLOVEMG3  very long",
      cmsOriginalFileExtension: ".pdf",
      mode: "read",
      pdfBlobName: undefined,
      presentationCategory: "Reviews",
      presentationFileName: "MCLOVEMG3  very long",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
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
        },
      ],
      sasUrl: undefined,
      url: undefined,
      polarisParentDocumentId: null,
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
      cmsDocumentId: "2",
      cmsOriginalFileName: "MCLOVEMG4 very long .docx",
      presentationTitle: "MCLOVEMG4 test",
      cmsOriginalFileExtension: ".pdf",
      mode: "read",
      pdfBlobName: undefined,
      presentationCategory: "Reviews",
      presentationFileName: "MCLOVEMG4 test",
      polarisDocumentVersionId: 1,
      categoryListOrder: null,
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
        },
      ],
      sasUrl: undefined,
      url: undefined,
      polarisParentDocumentId: null,
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
        presentationFileName: "MCLOVEMG3  very long",
      },
      {
        documentId: "2",
        presentationFileName: "MCLOVEMG4 test",
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
