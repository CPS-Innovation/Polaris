import { act, render, screen, waitFor } from "@testing-library/react";
import { InboundHandoverHandler } from "./InboundHandoverHandler";
import { RouteComponentProps } from "react-router-dom";
import * as api from "../features/cases/api/gateway-api";
import { ErrorBoundary } from "../common/presentation/components";

describe("InboundHandoverHandler", () => {
  const mockPush = jest.fn();

  const generateMockProps = (search: string) =>
    ({
      location: { search },
      history: { push: mockPush as RouteComponentProps["history"]["push"] },
    } as RouteComponentProps);

  beforeEach(() => {
    jest.spyOn(console, "error").mockImplementation(jest.fn());
  });
  beforeEach(jest.resetAllMocks);
  afterAll(jest.resetAllMocks);

  it("will navigate a handover object containing only a only a case id", async () => {
    jest.spyOn(api, "lookupUrn").mockResolvedValue({
      urn: "x",
      urnRoot: "foo",
      id: 44,
    });

    const inputContextObject = {
      caseId: 1,
    };

    const mockProps = generateMockProps(
      `?ctx=${JSON.stringify(inputContextObject)}`
    );
    await act(async () => {
      render(<InboundHandoverHandler {...mockProps} />);
    });

    expect(mockPush).toBeCalledWith(
      "/case-details/foo/44", // will prefer the caseId returned by the api call
      undefined
    );
  });

  it("will navigate a handover object containing only a only a case id and a urn", async () => {
    const inputContextObject = {
      caseId: 1,
      urn: "bar",
    };

    const mockProps = generateMockProps(
      `?ctx=${JSON.stringify(inputContextObject)}`
    );
    await act(async () => {
      render(<InboundHandoverHandler {...mockProps} />);
    });

    expect(mockPush).toBeCalledWith("/case-details/bar/1", undefined);
  });

  it("will navigate a well formed triage handover object with no urn to the appropriate case and set context", async () => {
    jest.spyOn(api, "lookupUrn").mockResolvedValue({
      urn: "x",
      urnRoot: "foo",
      id: 44,
    });

    const inputContextObject = {
      caseId: 1,
      taskId: 2,
      taskTypeId: 3,
    };

    const expectedContextObject = {
      taskId: 2,
      taskTypeId: 3,
      contextType: "triage",
    };

    const mockProps = generateMockProps(
      `?ctx=${JSON.stringify(inputContextObject)}`
    );
    await act(async () => {
      render(<InboundHandoverHandler {...mockProps} />);
    });

    expect(mockPush).toBeCalledWith(
      "/case-details/foo/44?taskId=2&taskTypeId=3", // will prefer the caseId returned by the api call
      expectedContextObject
    );
  });

  it("will navigate a well formed triage handover object with a urn to the appropriate case and set context", async () => {
    const inputContextObject = {
      caseId: 1,
      urn: "bar",
      taskId: 2,
      taskTypeId: 3,
    };

    const expectedContextObject = {
      taskId: 2,
      taskTypeId: 3,
      contextType: "triage",
    };

    const mockProps = generateMockProps(
      `?ctx=${JSON.stringify(inputContextObject)}`
    );
    await act(async () => {
      render(<InboundHandoverHandler {...mockProps} />);
    });

    expect(mockPush).toBeCalledWith(
      "/case-details/bar/1?taskId=2&taskTypeId=3",
      expectedContextObject
    );
  });

  it("will throw if there is not a ctx param", () => {
    const mockProps = generateMockProps("?foo=");
    const test = () => render(<InboundHandoverHandler {...mockProps} />);

    expect(test).toThrow(
      /No ctx query parameter found when receiving an app handover/
    );
  });

  it("will throw if the ctx is not valid JSON", () => {
    const mockProps = generateMockProps("?ctx=xyz");
    const test = () => render(<InboundHandoverHandler {...mockProps} />);

    expect(test).toThrow(
      /Context object from handing-over app is not valid JSON/
    );
  });

  it("will throw if the ctx has no caseId", () => {
    const search = `?ctx=${JSON.stringify({ foo: "bar" })}`;

    const mockProps = generateMockProps(search);
    const test = () => render(<InboundHandoverHandler {...mockProps} />);

    expect(test).toThrow(
      /Context object from handing-over app is missing caseId/
    );
  });

  it("will throw if the ctx is not of a recognised form", () => {
    const search = `?ctx=${JSON.stringify({ caseId: 123, foo: "bar" })}`;

    const mockProps = generateMockProps(search);
    const test = () => render(<InboundHandoverHandler {...mockProps} />);

    expect(test).toThrow(
      /Context object from handing-over app is not of an expected form/
    );
  });
});
