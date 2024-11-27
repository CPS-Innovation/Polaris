import { useMemo, useRef, useEffect, useState } from "react";
import { useHistory, useLocation } from "react-router-dom";
import { Location } from "history";
import { CaseDocumentViewModel } from "../../cases/domain/CaseDocumentViewModel";
import { AsyncResult } from "../../../common/types/AsyncResult";
import { MappedCaseDocument } from "../../cases/domain/MappedCaseDocument";
export type UnSavedRedactionDoc = {
  documentId: CaseDocumentViewModel["documentId"];
  presentationTitle: string;
};

export const useNavigationAlert = (
  tabItems: CaseDocumentViewModel[],
  documentsState: AsyncResult<MappedCaseDocument[]>
): {
  showAlert: boolean;
  newPath: string;
  setShowAlert: React.Dispatch<React.SetStateAction<boolean>>;
  navigationUnblockHandle: React.MutableRefObject<any>;
  unSavedRedactionDocs: UnSavedRedactionDoc[];
} => {
  const [showAlert, setShowAlert] = useState(false);
  const [newPath, setNewPath] = useState("");
  const history = useHistory();
  const location = useLocation();
  const navigationUnblockHandle = useRef<any>();
  const unSavedRedactionDocs = useMemo((): {
    documentId: CaseDocumentViewModel["documentId"];
    presentationTitle: string;
  }[] => {
    const unsavedRedactedItems = tabItems
      .filter(
        (item) =>
          item.redactionHighlights.length + item.pageDeleteRedactions.length > 0
      )
      .map(({ documentId }) => documentId);

    const mappedDocuments =
      documentsState.status === "succeeded" ? documentsState.data : [];
    const redactedDocs = mappedDocuments
      .filter((doc) => unsavedRedactedItems.some((id) => id === doc.documentId))
      .map((item) => ({
        documentId: item.documentId!,
        presentationTitle: item.presentationTitle!,
      }));
    return redactedDocs;
  }, [tabItems, documentsState]);

  useEffect(() => {
    navigationUnblockHandle.current = history.block((tx: Location) => {
      if (location.pathname === tx.pathname) {
        return;
      }
      if (unSavedRedactionDocs.length) {
        setNewPath(`${tx.pathname}?${tx.search}`);
        setShowAlert(true);
        return false;
      }
    });
    return () => {
      navigationUnblockHandle.current && navigationUnblockHandle.current();
    };
  }, [unSavedRedactionDocs, location.pathname, history]);

  useEffect(() => {
    window.onbeforeunload = unSavedRedactionDocs.length
      ? (e) => {
          e.returnValue = "warn";
          return;
        }
      : null;
    return () => {
      window.onbeforeunload = null;
    };
  }, [unSavedRedactionDocs]);

  return {
    showAlert,
    setShowAlert,
    newPath,
    navigationUnblockHandle,
    unSavedRedactionDocs,
  };
};
