import { renderHook } from "@testing-library/react-hooks";
import { CaseDocumentViewModel } from "../../cases/domain/CaseDocumentViewModel";
import { useNavigationAlert } from "./useNavigationAlert";

describe("useNaviagtionAlert hook", () => {
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
      redactionHighlights: [],
      sasUrl: undefined,
      tabSafeId: "d0",
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
      redactionHighlights: [],
      sasUrl: undefined,
      tabSafeId: "d1",
      url: undefined,
    },
  ];

  it("should return the correct unSavedRedactionDocs array", () => {
    const { result: resultOne } = renderHook(() => useNavigationAlert([]));
    expect(resultOne.current.unSavedRedactionDocs).toEqual([]);
    const { result } = renderHook(() => useNavigationAlert(tabItems));
    expect(result.current.unSavedRedactionDocs).toEqual([
      {
        documentId: 1,
        tabSafeId: "d0",
        presentationFileName: "MCLOVEMG3  very long",
      },
      {
        documentId: 2,
        tabSafeId: "d1",
        presentationFileName: "MCLOVEMG4 test",
      },
    ]);
  });
});
