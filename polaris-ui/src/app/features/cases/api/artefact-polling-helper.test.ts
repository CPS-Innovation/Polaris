import { API_LOCAL_POLLING_DELAY_MS } from "../../../config";
import {
  artefactPollingHelper,
  ERROR_MESSAGES,
} from "./artefact-polling-helper";

jest.mock("../../../config", () => ({
  API_LOCAL_POLLING_DELAY_MS: 1,
  API_LOCAL_POLLING_RETRY_COUNT: 3,
}));

const fooUrl = "http://foo";
const barUrl = "http://bar";

const successResponse = (url: string) =>
  Promise.resolve({
    ok: true,
    status: 200,
    json: () => Promise.resolve({ success: true, url }),
  } as Response);

const errorResponse = Promise.resolve({
  ok: false,
  status: 500,
} as Response);

const continueResponse = Promise.resolve({
  ok: true,
  status: 202,
  json: () => Promise.resolve({ nextUrl: barUrl }),
} as Response);

const unexpectedStatusResponse = Promise.resolve({
  ok: true,
  status: 201,
} as Response);

const unexpectedShapedResponse = Promise.resolve({
  ok: true,
  status: 202,
  json: () => Promise.resolve({ nextUrl: "" }),
  text: () => Promise.resolve('{ nextUrl: "" }'),
} as Response);

describe.only("artefact-polling-helper", () => {
  it("can return a response when api returns data on first poll", async () => {
    const result = await artefactPollingHelper(
      (url) => (url === fooUrl ? successResponse(url) : errorResponse),
      fooUrl
    );

    expect(result).toEqual({ success: true, url: fooUrl });
  });

  it("can throw if api errors on first poll", async () => {
    const act = async () =>
      await artefactPollingHelper(
        (url) => (url === fooUrl ? errorResponse : successResponse(url)),
        fooUrl
      );

    await expect(act).rejects.toThrowError(
      new RegExp(ERROR_MESSAGES.FAILED_API)
    );
  });

  it("can throw if api returns an unexpected success status code", async () => {
    const act = async () =>
      await artefactPollingHelper(
        (url) =>
          url === fooUrl ? unexpectedStatusResponse : successResponse(url),
        fooUrl
      );

    await expect(act).rejects.toThrowError(
      new RegExp(ERROR_MESSAGES.UNEXPECTED_STATUS_CODE)
    );
  });

  it("can throw if api returns an unexpected shaped continuation response", async () => {
    const act = async () =>
      await artefactPollingHelper(
        (url) =>
          url === fooUrl ? unexpectedShapedResponse : successResponse(url),
        fooUrl
      );

    await expect(act).rejects.toThrowError(
      new RegExp(ERROR_MESSAGES.UNEXPECTED_CONTINUATION_RESULT)
    );
  });

  it("can poll a second time and retrieve data", async () => {
    const result = await artefactPollingHelper(
      (url) =>
        url === fooUrl
          ? continueResponse
          : url === barUrl
          ? successResponse(url)
          : errorResponse,
      fooUrl
    );

    expect(result).toEqual({ success: true, url: barUrl });
  });

  it("can poll a second time and throw if api errors on second poll", async () => {
    const act = async () =>
      await artefactPollingHelper(
        (url) =>
          url === fooUrl
            ? continueResponse
            : url === barUrl
            ? errorResponse
            : successResponse(url),
        fooUrl
      );

    await expect(act).rejects.toThrowError(
      new RegExp(ERROR_MESSAGES.FAILED_API)
    );
  });

  it("can wait only two times if API_LOCAL_POLLING_RETRY_COUNT === 3", async () => {
    const realSetTimeout = global.setTimeout.bind(global);
    const realClearTimeout = global.clearTimeout.bind(global);

    const setTimeoutSpy = jest.spyOn(global, "setTimeout").mockImplementation(((
      cb: (...args: any[]) => void,
      delay?: number,
      ...args: any[]
    ) => {
      if (typeof cb === "function") cb(...args);

      const handle = realSetTimeout(() => {}, 0);
      realClearTimeout(handle);
      return handle as unknown as NodeJS.Timeout;
    }) as unknown as typeof setTimeout);

    let callCount = 0;
    const act = async () =>
      await artefactPollingHelper((_url) => {
        callCount += 1;
        return continueResponse;
      }, fooUrl);

    let errorMessage: string | undefined = "";
    try {
      await act();
    } catch (ex: any) {
      errorMessage = ex?.toString();
    }

    expect(errorMessage).toContain(ERROR_MESSAGES.TOO_MANY_ATTEMPTS);
    expect(callCount).toBe(3);

    expect(setTimeoutSpy).toHaveBeenCalledTimes(2);
    expect(setTimeoutSpy).toHaveBeenLastCalledWith(
      expect.any(Function),
      API_LOCAL_POLLING_DELAY_MS
    );

    setTimeoutSpy.mockRestore();
  });
});
