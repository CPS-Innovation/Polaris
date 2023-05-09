import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { CombinedState } from "../../domain/CombinedState";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import { reducerAsyncActionHandlers } from "./reducer-async-action-handlers";
import * as api from "../../api/gateway-api";
import * as headerFactory from "../../api/header-factory";
import * as mapRedactionSaveRequest from "./map-redaction-save-request";
import { RedactionSaveRequest } from "../../domain/gateway/RedactionSaveRequest";

describe("reducerAsyncActionHandlers", () => {
  const dispatchMock = jest.fn();
  let combinedStateMock: CombinedState;

  afterEach(() => {
    jest.resetAllMocks();
    combinedStateMock = {} as CombinedState;
  });

  describe("REQUEST_OPEN_PDF_IN_NEW_TAB", () => {
    it("can open a pdf in a new tab", async () => {
      // arrange
      const getPdfSasUrlSpy = jest
        .spyOn(api, "getPdfSasUrl")
        .mockImplementation(() => Promise.resolve("baz"));

      combinedStateMock = {
        urn: "foo",
        caseId: 99,
        tabsState: {
          items: [
            { documentId: "1", pdfBlobName: "bar1" },
            { documentId: "2", pdfBlobName: "bar2" },
          ] as CaseDocumentViewModel[],
        },
      } as CombinedState;

      const handler = reducerAsyncActionHandlers.REQUEST_OPEN_PDF_IN_NEW_TAB({
        dispatch: dispatchMock,
        getState: () => combinedStateMock,
        signal: new AbortController().signal,
      });

      // act
      await handler({
        type: "REQUEST_OPEN_PDF_IN_NEW_TAB",
        payload: {
          documentId: "1",
        },
      });

      // assert
      expect(getPdfSasUrlSpy).toBeCalledWith("foo", 99, "1");

      expect(dispatchMock.mock.calls.length).toBe(1);
      expect(dispatchMock.mock.calls[0][0]).toEqual({
        type: "OPEN_PDF_IN_NEW_TAB",
        payload: {
          documentId: "1",
          sasUrl: "baz",
        },
      });
    });
  });

  describe("REQUEST_OPEN_PDF", () => {
    it("can open a pdf when auth token and correlation id are retrieved", async () => {
      jest
        .spyOn(headerFactory, "correlationId")
        .mockImplementation(() => ({ "Correlation-Id": "foo" }));

      jest
        .spyOn(headerFactory, "auth")
        .mockImplementation(() => Promise.resolve({ Authorization: "bar" }));

      const handler = reducerAsyncActionHandlers.REQUEST_OPEN_PDF({
        dispatch: dispatchMock,
        getState: () => combinedStateMock,
        signal: new AbortController().signal,
      });

      // act
      await handler({
        type: "REQUEST_OPEN_PDF",
        payload: {
          documentId: "1",
          mode: "read",
        },
      });

      // assert
      expect(dispatchMock.mock.calls.length).toBe(1);
      expect(dispatchMock.mock.calls[0][0]).toEqual({
        type: "OPEN_PDF",
        payload: {
          documentId: "1",
          mode: "read",
          headers: {
            "Correlation-Id": "foo",
            Authorization: "bar",
          },
        },
      });
    });
  });

  describe("ADD_REDACTION_AND_POTENTIALLY_LOCK", () => {
    it.each<
      [
        CaseDocumentViewModel["clientLockedState"],
        boolean,
        CaseDocumentViewModel["clientLockedState"]
      ]
    >([
      ["unlocked", true, "locked"],
      ["unlocking", true, "locked"],
      ["locked-by-other-user", true, "locked"],
    ])(
      "can add a redaction and lock the document if the document is unlocked or unlocking and document checkout is successfull",
      async (
        clientLockedState,
        isLockSuccessful,
        expectedFinalDispatchedLockedState
      ) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              { documentId: "1", clientLockedState, cmsDocCategory: "MGForm" },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkoutSpy = jest
          .spyOn(api, "checkoutDocument")
          .mockImplementation(() => Promise.resolve(isLockSuccessful));

        const handler =
          reducerAsyncActionHandlers.ADD_REDACTION_AND_POTENTIALLY_LOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
          payload: {
            documentId: "1",
            redaction: { type: "redaction" } as NewPdfHighlight,
          },
        });

        //assert
        expect(checkoutSpy).toBeCalledWith("foo", 2, "1");

        expect(dispatchMock.mock.calls.length).toBe(3);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "locking" },
        });
        expect(dispatchMock.mock.calls[1][0]).toEqual({
          type: "ADD_REDACTION",
          payload: { documentId: "1", redaction: { type: "redaction" } },
        });

        expect(dispatchMock.mock.calls[2][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId: "1",
            lockedState: expectedFinalDispatchedLockedState,
          },
        });
      }
    );

    it.each<
      [
        CaseDocumentViewModel["clientLockedState"],
        boolean,
        CaseDocumentViewModel["clientLockedState"]
      ]
    >([
      ["unlocked", false, "unlocked"],
      ["unlocking", false, "unlocked"],
      ["locked-by-other-user", false, "unlocked"],
    ])(
      "Should not add redaction, if the document is unlocked or unlocking and document checkout is unsuccessfull",
      async (
        clientLockedState,
        isLockSuccessful,
        expectedFinalDispatchedLockedState
      ) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              { documentId: "1", clientLockedState, cmsDocCategory: "MGForm" },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkoutSpy = jest
          .spyOn(api, "checkoutDocument")
          .mockImplementation(() => Promise.reject({ isLockSuccessful }));

        const handler =
          reducerAsyncActionHandlers.ADD_REDACTION_AND_POTENTIALLY_LOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
          payload: {
            documentId: "1",
            redaction: { type: "redaction" } as NewPdfHighlight,
          },
        });

        //assert
        expect(checkoutSpy).toBeCalledWith("foo", 2, "1");

        expect(dispatchMock.mock.calls.length).toBe(3);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "locking" },
        });
        expect(dispatchMock.mock.calls[1][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId: "1",
            lockedState: expectedFinalDispatchedLockedState,
          },
        });
        expect(dispatchMock.mock.calls[2][0]).toEqual({
          type: "SHOW_ERROR_MODAL",
          payload: {
            title: "Something went wrong!",
            message: "Failed to checkout document. Please try again later.",
          },
        });
      }
    );

    it.each<
      [
        CaseDocumentViewModel["clientLockedState"],
        CaseDocumentViewModel["clientLockedState"]
      ]
    >([
      ["unlocked", "locked-by-other-user"],
      ["unlocking", "locked-by-other-user"],
      ["locked-by-other-user", "locked-by-other-user"],
    ])(
      "Should not add redaction, if the document is unlocked or unlocking and document checkout is unsuccessfull because it is locked by another user statuscode(409)",
      async (clientLockedState, expectedFinalDispatchedLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              { documentId: "1", clientLockedState, cmsDocCategory: "MGForm" },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkoutSpy = jest
          .spyOn(api, "checkoutDocument")
          .mockImplementation(() => Promise.reject({ code: 409 }));

        const handler =
          reducerAsyncActionHandlers.ADD_REDACTION_AND_POTENTIALLY_LOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
          payload: {
            documentId: "1",
            redaction: { type: "redaction" } as NewPdfHighlight,
          },
        });

        //assert
        expect(checkoutSpy).toBeCalledWith("foo", 2, "1");

        expect(dispatchMock.mock.calls.length).toBe(3);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "locking" },
        });
        expect(dispatchMock.mock.calls[1][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: {
            documentId: "1",
            lockedState: expectedFinalDispatchedLockedState,
          },
        });
        expect(dispatchMock.mock.calls[2][0]).toEqual({
          type: "SHOW_ERROR_MODAL",
          payload: {
            title: "Failed to redact document",
            message:
              "It is not possible to redact as the document is already checked out by another user. Please try again later.",
          },
        });
      }
    );

    it.each<CaseDocumentViewModel["clientLockedState"]>(["locking", "locked"])(
      "can add a redaction and not lock the document if the document is already locked or locking",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              { documentId: "1", clientLockedState },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
        } as CombinedState;

        const checkoutSpy = jest
          .spyOn(api, "checkoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.ADD_REDACTION_AND_POTENTIALLY_LOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "ADD_REDACTION_AND_POTENTIALLY_LOCK",
          payload: {
            documentId: "1",
            redaction: { type: "redaction" } as NewPdfHighlight,
          },
        });

        //assert
        expect(checkoutSpy).not.toBeCalled();

        expect(dispatchMock.mock.calls.length).toBe(1);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "ADD_REDACTION",
          payload: { documentId: "1", redaction: { type: "redaction" } },
        });
      }
    );
  });

  describe("REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK", () => {
    it.each<CaseDocumentViewModel["clientLockedState"]>([
      "unlocked",
      "locking",
      "locked",
      "unlocking",
      "locked-by-other-user",
    ])(
      "can remove a redaction and not unlock the document if it is not the last redaction",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              {
                documentId: "1",
                clientLockedState,
                redactionHighlights: [{ id: "bar" }, { id: "baz" }],
              },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkInSpy = jest
          .spyOn(api, "cancelCheckoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
          payload: {
            documentId: "1",
            redactionId: "bar",
          },
        });

        //assert
        expect(checkInSpy).not.toBeCalled();

        expect(dispatchMock.mock.calls.length).toBe(1);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "REMOVE_REDACTION",
          payload: { documentId: "1", redactionId: "bar" },
        });
      }
    );

    it.each<CaseDocumentViewModel["clientLockedState"]>([
      "unlocked",
      "unlocking",
      "locked-by-other-user",
    ])(
      "can remove a redaction and not unlock the document if it is the last redaction but the document is not at an appropriate state",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              {
                documentId: "1",
                clientLockedState,
                redactionHighlights: [{ id: "bar" }],
              },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkInSpy = jest
          .spyOn(api, "cancelCheckoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
          payload: {
            documentId: "1",
            redactionId: "bar",
          },
        });

        //assert
        expect(checkInSpy).not.toBeCalled();

        expect(dispatchMock.mock.calls.length).toBe(1);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "REMOVE_REDACTION",
          payload: { documentId: "1", redactionId: "bar" },
        });
      }
    );

    it.each<CaseDocumentViewModel["clientLockedState"]>(["locking", "locked"])(
      "can remove a redaction and  unlock the document if it is the last redaction and the document is at an appropriate state",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              {
                documentId: "1",
                cmsDocumentId: "a",
                cmsDocCategory: "MGForm",
                clientLockedState,
                redactionHighlights: [{ id: "bar" }],
              },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkInSpy = jest
          .spyOn(api, "cancelCheckoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "REMOVE_REDACTION_AND_POTENTIALLY_UNLOCK",
          payload: {
            documentId: "1",
            redactionId: "bar",
          },
        });

        //assert
        expect(checkInSpy).toBeCalledWith("foo", 2, "1");

        expect(dispatchMock.mock.calls.length).toBe(3);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "REMOVE_REDACTION",
          payload: { documentId: "1", redactionId: "bar" },
        });
        expect(dispatchMock.mock.calls[1][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "unlocking" },
        });
        expect(dispatchMock.mock.calls[2][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "unlocked" },
        });
      }
    );
  });

  describe("REMOVE_ALL_REDACTIONS_AND_UNLOCK", () => {
    it.each<CaseDocumentViewModel["clientLockedState"]>([
      "unlocked",
      "unlocking",
      "locked-by-other-user",
    ])(
      "can remove all redactions and not unlock the document if it is not at an appropriate state",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              {
                documentId: "1",
                cmsDocCategory: "MGForm",
                clientLockedState,
                redactionHighlights: [{ id: "bar" }],
              },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkInSpy = jest
          .spyOn(api, "cancelCheckoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.REMOVE_ALL_REDACTIONS_AND_UNLOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
          payload: {
            documentId: "1",
          },
        });

        //assert
        expect(checkInSpy).not.toBeCalled();

        expect(dispatchMock.mock.calls.length).toBe(1);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "REMOVE_ALL_REDACTIONS",
          payload: { documentId: "1" },
        });
      }
    );

    it.each<CaseDocumentViewModel["clientLockedState"]>(["locking", "locked"])(
      "can remove all redactions and unlock the document it is at an appropriate state",
      async (clientLockedState) => {
        // arrange
        combinedStateMock = {
          tabsState: {
            items: [
              {
                documentId: "1",
                cmsDocumentId: "a",
                cmsDocCategory: "MGForm",
                clientLockedState,
                redactionHighlights: [{ id: "bar" }],
              },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkInSpy = jest
          .spyOn(api, "cancelCheckoutDocument")
          .mockImplementation(() => Promise.resolve(true));

        const handler =
          reducerAsyncActionHandlers.REMOVE_ALL_REDACTIONS_AND_UNLOCK({
            dispatch: dispatchMock,
            getState: () => combinedStateMock,
            signal: new AbortController().signal,
          });

        //act
        await handler({
          type: "REMOVE_ALL_REDACTIONS_AND_UNLOCK",
          payload: {
            documentId: "1",
          },
        });

        //assert
        expect(checkInSpy).toBeCalledWith("foo", 2, "1");

        expect(dispatchMock.mock.calls.length).toBe(3);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "REMOVE_ALL_REDACTIONS",
          payload: { documentId: "1" },
        });
        expect(dispatchMock.mock.calls[1][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "unlocking" },
        });
        expect(dispatchMock.mock.calls[2][0]).toEqual({
          type: "UPDATE_DOCUMENT_LOCK_STATE",
          payload: { documentId: "1", lockedState: "unlocked" },
        });
      }
    );
  });

  describe("SAVE_REDACTIONS", () => {
    it("can save all redactions", async () => {
      // arrange
      const redactionHighlights = [{ id: "bar" }];

      combinedStateMock = {
        tabsState: {
          items: [
            {
              documentId: "1",
              cmsDocumentId: "a",
              cmsDocCategory: "MGForm",
              redactionHighlights,
              pdfBlobName: "baz",
            },
          ] as CaseDocumentViewModel[],
        },
        caseId: 2,
        urn: "foo",
      } as CombinedState;

      const saveSpy = jest
        .spyOn(api, "saveRedactions")
        .mockImplementation(() => Promise.resolve());

      const mockRedactionSaveRequest = {} as RedactionSaveRequest;

      jest
        .spyOn(mapRedactionSaveRequest, "mapRedactionSaveRequest")
        .mockImplementation((documentId, redactions) => {
          if (documentId === "1" && redactions === redactionHighlights) {
            return mockRedactionSaveRequest;
          }
          throw new Error(
            "mapRedactionSaveRequest mock received unexpected args"
          );
        });

      jest.spyOn(window, "open").mockImplementation(() => null);

      const handler = reducerAsyncActionHandlers.SAVE_REDACTIONS({
        dispatch: dispatchMock,
        getState: () => combinedStateMock,
        signal: new AbortController().signal,
      });

      //act
      await handler({
        type: "SAVE_REDACTIONS",
        payload: {
          documentId: "1",
        },
      });

      // assert
      expect(saveSpy).toBeCalledWith("foo", 2, "1", mockRedactionSaveRequest);
      //expect(checkInSpy).toBeCalledWith("foo", 2, 1);
    });
  });
});
