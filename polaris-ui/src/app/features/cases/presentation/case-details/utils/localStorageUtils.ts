import { IPdfHighlight } from "../../../domain/IPdfHighlight";

export type LocalStorageKey = "redactions" | "read";
export type ReadData = Record<string, boolean>;

type StorageData = {
  redactions: IPdfHighlight[];
  read: ReadData;
};

export const addToLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey,
  value: any
) => {
  const storageKey = `${caseId}`;

  const storedData = localStorage.getItem(storageKey);
  if (storedData) {
    const parsedData = JSON.parse(storedData);
    const newData = {
      ...parsedData,
      modifiedDate: Date.now(),
      value: { ...parsedData.value, [featureKey]: value },
    };
    localStorage.setItem(storageKey, JSON.stringify(newData));
    return;
  }

  localStorage.setItem(
    storageKey,
    JSON.stringify({ modifiedDate: Date.now(), value: { [featureKey]: value } })
  );
};

export const clearDownStorage = () => {
  const currentDate = new Date();
  const oneMonthAgo = new Date(
    currentDate.getTime() - 30 * 24 * 60 * 60 * 1000
  );

  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i)!;
    const value = localStorage.getItem(key)!;

    const item = JSON.parse(value);
    const itemTimestamp = new Date(item.modifiedDate);

    // Check if the item is older than 30 days
    if (itemTimestamp < oneMonthAgo) {
      localStorage.removeItem(key);
    }
  }
};

export const readFromLocalStorage = (
  caseId: number,
  featureKey: LocalStorageKey
) => {
  const storageKey = `${caseId}`;
  const storedData = localStorage.getItem(storageKey);
  if (storedData) {
    const data: StorageData = JSON.parse(storedData).value;
    if (data[featureKey]) {
      return data[featureKey];
    }
    return null;
  }
  return null;
};
