import { IPdfHighlight } from "../../../domain/IPdfHighlight";

const LOCAL_STORAGE_EXPIRY_DAYS = 30;

export type LocalStorageKey = "redactions" | "readUnread";
export type ReadUnreadData = string[];
export type RedactionsData = {
  documentId: string;
  redactionHighlights: IPdfHighlight[];
}[];

type StorageData = {
  redactions: RedactionsData;
  readUnread: ReadUnreadData;
};

export const addToLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey,
  value: any
) => {
  const storageKey = `caseworkApp-${caseId}`;

  const storedData = localStorage.getItem(storageKey);
  if (storedData) {
    const parsedData = JSON.parse(storedData);
    const newData = {
      ...parsedData,
      modifiedDate: Date.now(),
      [featureKey]: value,
    };
    localStorage.setItem(storageKey, JSON.stringify(newData));
    return;
  }

  localStorage.setItem(
    storageKey,
    JSON.stringify({ modifiedDate: Date.now(), [featureKey]: value })
  );
};

export const clearDownStorage = () => {
  const currentDate = new Date();
  const expiryTime = new Date(
    currentDate.getTime() - LOCAL_STORAGE_EXPIRY_DAYS * 24 * 60 * 60 * 1000
  );

  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i)!;
    if (key && !key.includes("caseworkApp-")) {
      return;
    }
    const value = localStorage.getItem(key)!;
    const item = JSON.parse(value);
    const itemTimestamp = new Date(item.modifiedDate);

    // Check if the item is older than LOCAL_STORAGE_EXPIRY_DAYS days
    if (itemTimestamp < expiryTime) {
      localStorage.removeItem(key);
    }
  }
};

export const readFromLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey
) => {
  const storageKey = `caseworkApp-${caseId}`;
  const storedData = localStorage.getItem(storageKey);
  if (storedData) {
    const data: StorageData = JSON.parse(storedData);
    if (data[featureKey]) {
      return data[featureKey];
    }
    return null;
  }
  return null;
};

export const deleteFromLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey
) => {
  const storageKey = `caseworkApp-${caseId}`;
  const storedData = localStorage.getItem(storageKey);
  if (storedData) {
    const parsedData = JSON.parse(storedData);
    delete parsedData[featureKey];
    const newData = {
      ...parsedData,
      modifiedDate: Date.now(),
    };
    localStorage.setItem(storageKey, JSON.stringify(newData));
    return;
  }
};
