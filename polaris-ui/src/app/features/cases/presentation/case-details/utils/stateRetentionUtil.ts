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
  // console.log("sessionStorage Size in MB", getTotalSessionStorageSize());
  try {
    const session_storage_key_prefix = "casework_app_state_";
    removeKeysWithPrefixExcept(
      session_storage_key_prefix,
      `${session_storage_key_prefix}${caseId}`
    );
    const serializedState = sessionStorage.getItem(
      `${session_storage_key_prefix}${caseId}`
    );
    if (serializedState === null) return null;

    return JSON.parse(serializedState) as CombinedState;
  } catch (error) {
    console.error("Could not retrieve state from sessionStorage:", error);
    return null;
  }
};

export const removeKeysWithPrefixExcept = (
  prefix: string,
  keyToRetain: string
) => {
  for (let i = sessionStorage.length - 1; i >= 0; i--) {
    const key = sessionStorage.key(i);
    if (key && key.startsWith(prefix) && key !== keyToRetain) {
      sessionStorage.removeItem(key);
    }
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
