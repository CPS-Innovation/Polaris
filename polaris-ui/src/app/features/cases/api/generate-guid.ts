import { v4 as uuidv4 } from "uuid";

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string;
}

export const generateGuid = () =>
  window.__POLARIS_INSTRUMENTATION_GUID__ || uuidv4();
