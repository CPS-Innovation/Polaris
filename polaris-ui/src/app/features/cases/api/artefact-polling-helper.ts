import { ApiError } from "../../../common/errors/ApiError";
import {
  API_LOCAL_POLLING_DELAY_MS,
  API_LOCAL_POLLING_RETRY_COUNT,
} from "../../../config";

export const ERROR_MESSAGES = {
  FAILED_API: "api response error",
  TOO_MANY_ATTEMPTS: "too many polling attempts",
  UNEXPECTED_STATUS_CODE: "unexpected success code received",
  UNEXPECTED_CONTINUATION_RESULT: `expected a response containing nextUrl e.g. { "nextUrl": "https:/foo" } but received`,
};

export const artefactPollingHelper = async <T>(
  getData: (url: string) => Promise<Response>,
  initialUrl: string
) => {
  const internal = async <T>(
    url: string,
    retriesLeft: number = API_LOCAL_POLLING_RETRY_COUNT - 1
  ): Promise<T> => {
    const response = await getData(url);

    return (
      (await tryHandleApiError(response, url)) ||
      (await tryHandleSuccess<T>(response, url)) ||
      (await tryHandleRetryLimit(response, url, retriesLeft)) ||
      (await tryHandleUnexpectedStatusCode(response, url)) ||
      (await tryHandleUnexpectedShapedResponseCode(response, url)) ||
      (await waitAndRecurse<T>(response, url, retriesLeft, internal))
    );
  };

  return internal<T>(initialUrl);
};

const tryHandleApiError = async (response: Response, url: string) => {
  if (!response.ok) {
    throw new ApiError(
      "Getting artefact failed: api response error",
      url,
      response
    );
  } else {
    return undefined;
  }
};

const tryHandleSuccess = async <T>(response: Response, url: string) => {
  if (response.status === 200) {
    return (await response.json()) as T;
  } else {
    return undefined;
  }
};

const tryHandleRetryLimit = async (
  response: Response,
  url: string,
  retriesLeft: number
) => {
  if (retriesLeft <= 0) {
    throw new ApiError(
      `Getting artefact failed: ${ERROR_MESSAGES.TOO_MANY_ATTEMPTS} (${API_LOCAL_POLLING_RETRY_COUNT})`,
      url,
      response
    );
  } else {
    return undefined;
  }
};

const tryHandleUnexpectedStatusCode = async (
  response: Response,
  url: string
) => {
  if (response.status !== 202) {
    throw new ApiError(
      `Getting artefact failed: ${ERROR_MESSAGES.UNEXPECTED_STATUS_CODE} ${response.status}`,
      url,
      response
    );
  } else {
    return undefined;
  }
};

const tryHandleUnexpectedShapedResponseCode = async (
  response: Response,
  url: string
) => {
  const { nextUrl } = (await response.json()) as { nextUrl: string };

  if (!nextUrl) {
    throw new ApiError(
      `Getting artefact failed: ${
        ERROR_MESSAGES.UNEXPECTED_CONTINUATION_RESULT
      } ${await response.text()}`,
      url,
      response
    );
  } else {
    return undefined;
  }
};

const waitAndRecurse = async <T>(
  response: Response,
  url: string,
  retriesLeft: number,
  fn: <T>(url: string, retriesLeft: number) => Promise<T>
) => {
  const { nextUrl } = (await response.json()) as { nextUrl: string };

  await new Promise((resolve) =>
    setTimeout(resolve, API_LOCAL_POLLING_DELAY_MS)
  );

  return await fn<T>(nextUrl, retriesLeft - 1);
};