import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import { LOCAL_STORAGE_EXPIRY_DAYS } from "../../../../../config";

const LOCAL_STORAGE_PREFIX = "polaris-";

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
  const storageKey = `${LOCAL_STORAGE_PREFIX}${caseId}`;

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
  if (LOCAL_STORAGE_EXPIRY_DAYS === null) {
    return;
  }
  const oneDayMilliseconds = 24 * 60 * 60 * 1000;
  const currentDate = new Date();
  const expiryTime = new Date(
    currentDate.getTime() - LOCAL_STORAGE_EXPIRY_DAYS * oneDayMilliseconds
  );
  const expiredKeys = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i)!;
    if (key && !key.includes(LOCAL_STORAGE_PREFIX)) {
      continue;
    }
    const value = localStorage.getItem(key)!;
    const item = JSON.parse(value);
    const itemTimestamp = new Date(item.modifiedDate);
    if (itemTimestamp < expiryTime) {
      expiredKeys.push(key);
    }
  }
  // Check if the item is older than LOCAL_STORAGE_EXPIRY_DAYS days
  expiredKeys.forEach((key) => {
    localStorage.removeItem(key);
  });
};

export const readFromLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey
) => {
  const storageKey = `${LOCAL_STORAGE_PREFIX}${caseId}`;
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
  const storageKey = `${LOCAL_STORAGE_PREFIX}${caseId}`;
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
