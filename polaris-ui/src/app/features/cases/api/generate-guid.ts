import { v4 as uuidv4, validate } from "uuid";

declare global {
  var __POLARIS_INSTRUMENTATION_GUID__: string;
  var __POLARIS_TEST__: boolean;
}

window.__POLARIS_TEST__ = true;

export const generateGuid = () => {
  console.log("Generating guid " + window.__POLARIS_INSTRUMENTATION_GUID__);
  if (window.__POLARIS_INSTRUMENTATION_GUID__) {
    if (!validate(window.__POLARIS_INSTRUMENTATION_GUID__)) {
      throw new Error(
        "The __POLARIS_INSTRUMENTATION_GUID__ window variable has been set but is not a valid UUID"
      );
    }
    return window.__POLARIS_INSTRUMENTATION_GUID__;
  }
  return uuidv4();
};
