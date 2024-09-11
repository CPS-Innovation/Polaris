import { render } from "@testing-library/react";
import { InboundHandoverHandler } from "./InboundHandoverHandler";
import { RouteComponentProps } from "react-router-dom";

jest.mock("../features/cases/api/gateway-api", () => ({
  lookupUrn: (id: number) =>
    Promise.resolve({
      urnRoot: "a",
      caseId: 4,
    }),
}));

describe("InboundHandoverHandler", () => {
  const mockPush: RouteComponentProps["history"]["push"] = jest.fn();

  const generateMockProps = (search: string) =>
    ({
      location: { search },
      history: { push: mockPush },
    } as RouteComponentProps);

  beforeEach(jest.resetAllMocks);
  afterAll(jest.resetAllMocks);

  it("will navigate a well formed triage handover to the appropriate case and set context", () => {
    const contextObject = {
      caseId: 1,
      taskId: 2,
      taskTypeId: 3,
    };

    const search = `?ctx=${encodeURIComponent(JSON.stringify(contextObject))}`;

    const mockProps = generateMockProps(search);
    render(<InboundHandoverHandler {...mockProps} />);
  });
});
