import { render, screen } from "@testing-library/react";
import { TrackerSummary } from "./TrackerSummary";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

describe("TrackerSummary", () => {
  it("Should return null if the there is not documents data available", () => {
    render(
      <TrackerSummary
        pipelineState={{ haveData: false } as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={false}
      />
    );
    expect(screen.queryByTestId("tracker-summary")).not.toBeInTheDocument();
  });
  it("Should render the tracker summary correctly if all the documents are indexed", () => {
    const pipelineState = {
      haveData: true,
      data: {
        status: "Completed",
        documents: [
          {
            status: "Indexed",
          },
          {
            status: "Indexed",
          },
          {
            status: "Indexed",
          },
        ],
      },
    };
    render(
      <TrackerSummary
        pipelineState={pipelineState as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={true}
      />
    );

    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 3"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 3"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 3"
    );
    expect(screen.queryByText("Case is ready to search")).toBeInTheDocument();
  });
  it("Should render the tracker summary correctly if some documents are not indexed", () => {
    const pipelineState = {
      haveData: true,
      data: {
        status: "Completed",
        documents: [
          {
            status: "Indexed",
          },
          {
            status: "Indexed",
          },
          {
            status: "OcrAndIndexFailure",
          },
        ],
      },
    };
    render(
      <TrackerSummary
        pipelineState={pipelineState as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={true}
      />
    );

    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 3"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 3"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 2 (unable to index 1 document)"
    );
    expect(screen.queryByText("Case is ready to search")).toBeInTheDocument();
  });
  it("Should render the tracker summary correctly if some documents are not ready to read", () => {
    const pipelineState = {
      haveData: true,
      data: {
        status: "Completed",
        documents: [
          {
            status: "UnexpectedFailure",
          },
          {
            status: "Indexed",
          },
          {
            status: "UnableToConvertToPdf",
          },
          {
            status: "New",
          },
        ],
      },
    };
    render(
      <TrackerSummary
        pipelineState={pipelineState as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={true}
      />
    );

    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 4"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 1 (unable to convert 3 documents)"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 1 (unable to index 3 documents)"
    );
    expect(screen.queryByText("Case is ready to search")).toBeInTheDocument();
  });
  it("Should ignore the DAC documents from tracker summary if case is not multipleDefendants", () => {
    const pipelineState = {
      haveData: true,
      data: {
        status: "Completed",
        documents: [
          {
            status: "Indexed",
          },
          {
            status: "Indexed",
          },
          {
            status: "Indexed",
          },
        ],
      },
    };
    render(
      <TrackerSummary
        pipelineState={pipelineState as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={false}
      />
    );

    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 2"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 2"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 2"
    );
    expect(screen.queryByText("Case is ready to search")).toBeInTheDocument();
  });
  it("Should render the tracker summary correctly if pipelineState is not completed", () => {
    const pipelineState = {
      haveData: true,
      data: {
        status: "Running",
        documents: [
          {
            status: "Indexed",
          },
          {
            status: "UnexpectedFailure",
          },
          {
            status: "Indexed",
          },
        ],
      },
    };
    render(
      <TrackerSummary
        pipelineState={pipelineState as CaseDetailsState["pipelineState"]}
        isMultipleDefendantsOrCharges={false}
      />
    );
    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 2"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 1"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 1"
    );
    expect(
      screen.queryByText("Case is not ready to search")
    ).toBeInTheDocument();
  });
});
