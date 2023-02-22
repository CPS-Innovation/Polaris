import { useMemo, useRef, useEffect, useState } from "react";
import { useHistory, useLocation } from "react-router-dom";
import { Location } from "history";
import { CaseDocumentViewModel } from "../../cases/domain/CaseDocumentViewModel";
export type UnSavedRedactionDoc = {
  documentId: CaseDocumentViewModel["documentId"];
  tabSafeId: string;
  presentationFileName: string;
};

export const useNavigationAlert = (
  tabItems: CaseDocumentViewModel[]
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
    tabSafeId: string;
    presentationFileName: string;
  }[] => {
    const reactionPdfs = tabItems
      .filter((item) => item.redactionHighlights.length > 0)
      .map((item) => ({
        documentId: item.documentId!,
        tabSafeId: item.tabSafeId!,
        presentationFileName: item.presentationFileName!,
      }));
    return reactionPdfs;
  }, [tabItems]);

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
