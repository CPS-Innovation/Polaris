import { CaseDocumentViewModel } from "../../domain/CaseDocumentViewModel";
import { CombinedState } from "../../domain/CombinedState";
import { NewPdfHighlight } from "../../domain/NewPdfHighlight";
import {
  reducerAsyncActionHandlers,
  CHECKOUT_BLOCKED_STATUS_CODE,
} from "./reducer-async-action-handlers";
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
              { documentId: "1", clientLockedState },
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
            redactions: [{ type: "redaction" }] as NewPdfHighlight[],
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
          payload: { documentId: "1", redactions: [{ type: "redaction" }] },
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
              { documentId: "1", clientLockedState },
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
            redactions: [{ type: "redaction" }] as NewPdfHighlight[],
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
            type: "documentcheckout",
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
              { documentId: "1", clientLockedState },
            ] as CaseDocumentViewModel[],
          },
          caseId: 2,
          urn: "foo",
        } as CombinedState;

        const checkoutSpy = jest
          .spyOn(api, "checkoutDocument")
          .mockImplementation(() =>
            Promise.reject({
              code: CHECKOUT_BLOCKED_STATUS_CODE,
              customProperties: { username: "test_username" },
            })
          );

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
            redactions: [{ type: "redaction" }] as NewPdfHighlight[],
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
            type: "documentalreadycheckedout",
            title: "Failed to redact document",
            message:
              "It is not possible to redact as the document is already checked out by test_username. Please try again later.",
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
            redactions: [{ type: "redaction" }] as NewPdfHighlight[],
          },
        });

        //assert
        expect(checkoutSpy).not.toBeCalled();

        expect(dispatchMock.mock.calls.length).toBe(1);
        expect(dispatchMock.mock.calls[0][0]).toEqual({
          type: "ADD_REDACTION",
          payload: { documentId: "1", redactions: [{ type: "redaction" }] },
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

  describe("UNLOCK_DOCUMENTS", () => {
    it("it can unlock all the documents passed in", async () => {
      // arrange
      const cancelCheckoutSpy = jest
        .spyOn(api, "cancelCheckoutDocument")
        .mockImplementation(() => Promise.resolve(true));

      combinedStateMock = {
        urn: "foo",
        caseId: 99,
        tabsState: {
          items: [
            { documentId: "1", pdfBlobName: "bar1" },
            { documentId: "2", pdfBlobName: "bar2" },
            { documentId: "3", pdfBlobName: "bar3" },
          ] as CaseDocumentViewModel[],
        },
      } as CombinedState;

      const handler = reducerAsyncActionHandlers.UNLOCK_DOCUMENTS({
        dispatch: dispatchMock,
        getState: () => combinedStateMock,
        signal: new AbortController().signal,
      });

      // act
      await handler({
        type: "UNLOCK_DOCUMENTS",
        payload: {
          documentIds: ["1", "2", "3"],
        },
      });

      // assert
      expect(cancelCheckoutSpy).toBeCalledTimes(3);
      expect(cancelCheckoutSpy).toBeCalledWith("foo", 99, "1");
      expect(cancelCheckoutSpy).toBeCalledWith("foo", 99, "2");
      expect(cancelCheckoutSpy).toBeCalledWith("foo", 99, "3");
    });
    it("it shouldn't make api call to unlock if no documentIds passed in", async () => {
      // arrange
      const cancelCheckoutSpy = jest
        .spyOn(api, "cancelCheckoutDocument")
        .mockImplementation(() => Promise.resolve(true));

      combinedStateMock = {
        urn: "foo",
        caseId: 99,
        tabsState: {
          items: [
            { documentId: "1", pdfBlobName: "bar1" },
            { documentId: "2", pdfBlobName: "bar2" },
            { documentId: "3", pdfBlobName: "bar3" },
          ] as CaseDocumentViewModel[],
        },
      } as CombinedState;

      const handler = reducerAsyncActionHandlers.UNLOCK_DOCUMENTS({
        dispatch: dispatchMock,
        getState: () => combinedStateMock,
        signal: new AbortController().signal,
      });

      // act
      await handler({
        type: "UNLOCK_DOCUMENTS",
        payload: {
          documentIds: [],
        },
      });

      // assert
      expect(cancelCheckoutSpy).toBeCalledTimes(0);
    });
  });
});
