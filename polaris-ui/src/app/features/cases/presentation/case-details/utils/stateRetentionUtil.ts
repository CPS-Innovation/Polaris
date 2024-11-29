import { CombinedState } from "../../../domain/CombinedState";
export const saveStateToSessionStorage = (state: CombinedState) => {
  try {
    const serializedState = JSON.stringify(state);
    sessionStorage.setItem(
      `casework_app_state_${state.caseId}`,
      serializedState
    );
  } catch (error) {
    console.error("Could not save state to sessionStorage:", error);
  }
};

export const getStateFromSessionStorage = (
  caseId: number
): CombinedState | null => {
  console.log("getStateFromSessionStorage called >>> ");
  console.log("sessionStorage Size in MB", getTotalSessionStorageSize());
  try {
    const serializedState = sessionStorage.getItem(
      `casework_app_state_${caseId}`
    );
    if (serializedState === null) return null;

    return JSON.parse(serializedState) as CombinedState;
  } catch (error) {
    console.error("Could not retrieve state from sessionStorage:", error);
    return null;
  }
};

const getTotalSessionStorageSize = () => {
  let totalSize = 0;

  for (let i = 0; i < sessionStorage.length; i++) {
    const key = sessionStorage.key(i);
    if (!key) return;
    const item = sessionStorage.getItem(key);
    if (item) {
      totalSize += new Blob([item]).size;
    }
  }

  return totalSize / (1024 * 1024); //size im MB
};
