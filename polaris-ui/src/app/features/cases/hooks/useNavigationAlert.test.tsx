import { renderHook } from "@testing-library/react-hooks";
import { CaseDocumentViewModel } from "../domain/CaseDocumentViewModel";
import { useNavigationAlert } from "./useNavigationAlert";
import { createMemoryHistory } from "history";
import { Router } from "react-router-dom";
import { AsyncResult } from "../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../cases/domain/MappedCaseDocument";
describe("useNavigationAlert hook", () => {
  const history = createMemoryHistory();
  const tabItems: CaseDocumentViewModel[] = [
    {
      documentId: "1",
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
    },
    {
      clientLockedState: "unlocked",
      documentId: "2",
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
    },
  ] as unknown as CaseDocumentViewModel[];
  const documentsState = {
    status: "succeeded",
    data: [
      {
        documentId: "1",
        presentationTitle: "MCLOVEMG3  very long",
      },
      {
        documentId: "2",
        presentationTitle: "MCLOVEMG4 test",
      },
    ],
  } as unknown as AsyncResult<MappedCaseDocument[]>;

  it("should return the correct unSavedRedactionDocs array", () => {
    const { result } = renderHook(
      () => useNavigationAlert(tabItems, documentsState),
      {
        wrapper: ({ children }) => (
          <Router history={history}>
            <div>{children}</div>
          </Router>
        ),
      }
    );
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

  it("Should return empty array if the there are no active redactions in any document items", () => {
    const { result: resultOne } = renderHook(
      () => useNavigationAlert([], documentsState),
      {
        wrapper: ({ children }) => (
          <>
            <Router history={history}>
              <div>{children}</div>
            </Router>
          </>
        ),
      }
    );
    expect(resultOne.current.unSavedRedactionDocs).toEqual([]);
  });
});
