import { CMS_AUTH_VALUES_SESSION_KEY } from "../../config";
import { FetchArgs } from "./core";

export const fetchWithAuthHeader = async (...args: FetchArgs) => {
  var requestInit = {
    ...args[1],
  } as RequestInit;

  var cmsAuthValues = sessionStorage.getItem(CMS_AUTH_VALUES_SESSION_KEY);

  if (cmsAuthValues) {
    requestInit.headers = { ...requestInit.headers, [CMS_AUTH_VALUES_SESSION_KEY]: cmsAuthValues };
  }

  return await fetch(args[0], requestInit);
};
