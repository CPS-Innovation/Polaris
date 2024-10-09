import { useEffect } from "react";
import {
  readFromLocalStorage,
  ReadUnreadData,
} from "../../presentation/case-details/utils/localStorageUtils";
import { DispatchType } from "./reducer";
import { AsyncResult } from "../../../../common/types/AsyncResult";

export const useHydrateFromLocalStorage = (
  caseId: number,
  storedUserDataStatus: AsyncResult<unknown>["status"],
  dispatch: DispatchType
) => {
  useEffect(() => {
    if (storedUserDataStatus === "loading") {
      const docReadData = readFromLocalStorage(
        caseId,
        "readUnread"
      ) as ReadUnreadData | null;
      dispatch({
        type: "UPDATE_STORED_USER_DATA",
        payload: {
          storedUserData: { readUnread: docReadData ?? [] },
        },
      });
    }
  }, [storedUserDataStatus, caseId, dispatch]);
};
