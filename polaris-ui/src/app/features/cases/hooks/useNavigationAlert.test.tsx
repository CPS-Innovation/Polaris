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
      cmsDocCategory: "MGForm",
      cmsDocType: { id: 3, code: "MG3", name: "MG3 File" },
      createdDate: "2020-06-02",
      documentId: 1,
      fileName: "MCLOVEMG3  very long .docx",
      mode: "read",
      pdfBlobName: undefined,
      presentationCategory: "Reviews",
      presentationFileName: "MCLOVEMG3  very long",
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
    },
    {
      clientLockedState: "unlocked",
      cmsDocCategory: "MGForm",
      cmsDocType: { id: 3, code: "MG3", name: "MG3 File" },
      createdDate: "2020-06-02",
      documentId: 2,
      fileName: "MCLOVEMG4 very long .docx",
      mode: "read",
      pdfBlobName: undefined,
      presentationCategory: "Reviews",
      presentationFileName: "MCLOVEMG4 test",
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
        documentId: 1,

        presentationFileName: "MCLOVEMG3  very long",
      },
      {
        documentId: 2,

        presentationFileName: "MCLOVEMG4 test",
      },
    ]);
  });

  it("Should return empty array if the there are no active redactions in any document items", () => {
    const { result: resultOne } = renderHook(() => useNavigationAlert([]), {
      wrapper: ({ children }) => (
        <>
          <Router history={history}>
            <div>{children}</div>
          </Router>
        </>
      ),
    });
    expect(resultOne.current.unSavedRedactionDocs).toEqual([]);
  });
});
