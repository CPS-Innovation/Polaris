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

type RereadableResponse = {
  response: Response;
  isJsonRead?: boolean;
  json?: any;
  isTextRead?: boolean;
  text?: string;
};

const readJson = async (rereadableResponse: RereadableResponse) => {
  if (!rereadableResponse.isJsonRead) {
    rereadableResponse.json = await rereadableResponse.response.json();
  }
  rereadableResponse.isJsonRead = true;
  return rereadableResponse.json;
};

const readText = async (rereadableResponse: RereadableResponse) => {
  if (!rereadableResponse.isTextRead) {
    rereadableResponse.text = await rereadableResponse.response.text();
  }
  rereadableResponse.isTextRead = true;
  return rereadableResponse.text;
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
    const rereadableResponse = { response } as RereadableResponse;

    return (
      (await tryHandleApiError(rereadableResponse, url)) ||
      (await tryHandleSuccess<T>(rereadableResponse, url)) ||
      (await tryHandleRetryLimit(rereadableResponse, url, retriesLeft)) ||
      (await tryHandleUnexpectedStatusCode(rereadableResponse, url)) ||
      (await tryHandleUnexpectedShapedResponseCode(rereadableResponse, url)) ||
      (await waitAndRecurse<T>(rereadableResponse, url, retriesLeft, internal))
    );
  };

  return internal<T>(initialUrl);
};

const tryHandleApiError = async (
  { response }: RereadableResponse,
  url: string
) => {
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

const tryHandleSuccess = async <T>(
  rereadableResponse: RereadableResponse,
  url: string
) => {
  if (rereadableResponse.response.status === 200) {
    return (await readJson(rereadableResponse)) as T;
  } else {
    return undefined;
  }
};

const tryHandleRetryLimit = async (
  { response }: RereadableResponse,
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
  { response }: RereadableResponse,
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
  rereadableResponse: RereadableResponse,
  url: string
) => {
  const { nextUrl } = (await readJson(rereadableResponse)) as {
    nextUrl: string;
  };

  if (!nextUrl) {
    throw new ApiError(
      `Getting artefact failed: ${
        ERROR_MESSAGES.UNEXPECTED_CONTINUATION_RESULT
      } ${await readText(rereadableResponse)}`,
      url,
      rereadableResponse.response
    );
  } else {
    return undefined;
  }
};

const waitAndRecurse = async <T>(
  rereadableResponse: RereadableResponse,
  url: string,
  retriesLeft: number,
  fn: <T>(url: string, retriesLeft: number) => Promise<T>
) => {
  const { nextUrl } = (await readJson(rereadableResponse)) as {
    nextUrl: string;
  };

  await new Promise((resolve) =>
    setTimeout(resolve, API_LOCAL_POLLING_DELAY_MS)
  );

  return await fn<T>(nextUrl, retriesLeft - 1);
};
