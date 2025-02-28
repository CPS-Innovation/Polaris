import { useGetDocumentsListApi } from "./useGetDocumentsListApi";
import * as useApi from "../../../../common/hooks/useApi";
import * as refreshCycleDataUpdate from "../utils/refreshCycleDataUpdate";
import { RenameDocumentData } from "../../domain/gateway/RenameDocumentData";
import { ReclassifyDocumentData } from "../../domain/gateway/ReclassifyDocumentData";
import { renderHook } from "@testing-library/react";

describe("useGetDocumentsListApi", () => {
  it("should fetch new documents and dispatch actions with correct payload", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, c) => {
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(refreshCycleDataUpdate, "handleRenameUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    jest
      .spyOn(refreshCycleDataUpdate, "handleReclassifyUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    const mockDispatch = jest.fn();
    const renameDocuments = [
      { documentId: "1", saveRenameRefreshStatus: "updating" },
    ] as RenameDocumentData[];
    const reclassifyDocuments = [
      { documentId: "1", saveReclassifyRefreshStatus: "updating" },
    ] as ReclassifyDocumentData[];
    renderHook(() =>
      useGetDocumentsListApi(
        "0",
        1,
        true,
        renameDocuments,
        reclassifyDocuments,
        mockDispatch
      )
    );

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(3);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_DOCUMENTS",
      payload: { data: [{ id: 1 }], status: "succeeded" },
    });
    expect(mockDispatch).toHaveBeenNthCalledWith(2, {
      type: "UPDATE_RENAME_DATA",
      payload: {
        properties: {
          documentId: "1",
          saveRenameRefreshStatus: "updated",
        },
      },
    });
    expect(mockDispatch).toHaveBeenNthCalledWith(3, {
      type: "UPDATE_RECLASSIFY_DATA",
      payload: {
        properties: {
          documentId: "1",
          saveReclassifyRefreshStatus: "updated",
        },
      },
    });
  });

  it("should not dispatch UPDATE_RENAME_DATA action if handleRenameUpdateConfirmation return false ", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, c) => {
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(refreshCycleDataUpdate, "handleRenameUpdateConfirmation")
      .mockImplementation((a, b) => {
        return false;
      });
    jest
      .spyOn(refreshCycleDataUpdate, "handleReclassifyUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    const mockDispatch = jest.fn();
    const renameDocuments = [
      { documentId: "1", saveRenameRefreshStatus: "updating" },
    ] as RenameDocumentData[];
    const reclassifyDocuments = [
      { documentId: "1", saveReclassifyRefreshStatus: "updating" },
    ] as ReclassifyDocumentData[];
    renderHook(() =>
      useGetDocumentsListApi(
        "0",
        1,
        true,
        renameDocuments,
        reclassifyDocuments,
        mockDispatch
      )
    );

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(2);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_DOCUMENTS",
      payload: { data: [{ id: 1 }], status: "succeeded" },
    });

    expect(mockDispatch).toHaveBeenNthCalledWith(2, {
      type: "UPDATE_RECLASSIFY_DATA",
      payload: {
        properties: {
          documentId: "1",
          saveReclassifyRefreshStatus: "updated",
        },
      },
    });
  });

  it("should not dispatch UPDATE_RECLASSIFY_DATA action if handleReclassifyUpdateConfirmation return false ", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, c) => {
      return { status: "succeeded", data: [{ id: 1 }] as any };
    });
    jest
      .spyOn(refreshCycleDataUpdate, "handleRenameUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    jest
      .spyOn(refreshCycleDataUpdate, "handleReclassifyUpdateConfirmation")
      .mockImplementation((a, b) => {
        return false;
      });
    const mockDispatch = jest.fn();
    const renameDocuments = [
      { documentId: "1", saveRenameRefreshStatus: "updating" },
    ] as RenameDocumentData[];
    const reclassifyDocuments = [
      { documentId: "1", saveReclassifyRefreshStatus: "updating" },
    ] as ReclassifyDocumentData[];
    renderHook(() =>
      useGetDocumentsListApi(
        "0",
        1,
        true,
        renameDocuments,
        reclassifyDocuments,
        mockDispatch
      )
    );

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(2);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_DOCUMENTS",
      payload: { data: [{ id: 1 }], status: "succeeded" },
    });

    expect(mockDispatch).toHaveBeenNthCalledWith(2, {
      type: "UPDATE_RENAME_DATA",
      payload: {
        properties: {
          documentId: "1",
          saveRenameRefreshStatus: "updated",
        },
      },
    });
  });

  it("should fetch new documents and dispatch only the UPDATE_DOCUMENTS action if the api fetch is not complete yet ", async () => {
    jest.spyOn(useApi, "useApi").mockImplementation((a, b, c) => {
      return { status: "loading" };
    });
    jest
      .spyOn(refreshCycleDataUpdate, "handleRenameUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    jest
      .spyOn(refreshCycleDataUpdate, "handleReclassifyUpdateConfirmation")
      .mockImplementation((a, b) => {
        return true;
      });
    const mockDispatch = jest.fn();
    const renameDocuments = [
      { documentId: "1", saveRenameRefreshStatus: "updating" },
    ] as RenameDocumentData[];
    const reclassifyDocuments = [
      { documentId: "1", saveReclassifyRefreshStatus: "updating" },
    ] as ReclassifyDocumentData[];
    renderHook(() =>
      useGetDocumentsListApi(
        "0",
        1,
        true,
        renameDocuments,
        reclassifyDocuments,
        mockDispatch
      )
    );

    expect(useApi.useApi).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenCalledTimes(1);
    expect(mockDispatch).toHaveBeenNthCalledWith(1, {
      type: "UPDATE_DOCUMENTS",
      payload: { status: "loading" },
    });
  });
});
