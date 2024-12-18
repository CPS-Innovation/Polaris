import { v4 as uuidv4 } from "uuid";

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string;
}

export const generateGuid = () => {
  console.log(
    "Being asked for correlation Id and by override is:",
    window.__POLARIS_INSTRUMENTATION_GUID__
  );
  return window.__POLARIS_INSTRUMENTATION_GUID__ || uuidv4();
};
