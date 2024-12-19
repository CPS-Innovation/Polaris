import { render, screen } from "@testing-library/react";
import { TrackerSummary } from "./TrackerSummary";
import { CaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";

describe("TrackerSummary", () => {
  it("Should return null if the there is not documents data available", () => {
    render(
      <TrackerSummary pipelineState={{} as CaseDetailsState["pipelineState"]} />
    );
    expect(screen.queryByTestId("tracker-summary")).not.toBeInTheDocument();
  });
  it("Should render the tracker summary correctly if all the documents are indexed", () => {
    const pipelineState = {
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
  });
  it("Should render the tracker summary correctly if some documents are not indexed", () => {
    const pipelineState = {
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
  });
  it("Should render the tracker summary correctly if some documents are not ready to read", () => {
    const pipelineState = {
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
  });
  it("Should render the tracker summary correctly if pipelineState is not completed", () => {
    const pipelineState = {
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
      />
    );
    expect(screen.queryByTestId("tracker-summary")).toBeInTheDocument();
    expect(screen.queryByText("Total documents:")).toHaveTextContent(
      "Total documents: 3"
    );
    expect(screen.queryByText("Documents ready to read:")).toHaveTextContent(
      "Documents ready to read: 2"
    );
    expect(screen.queryByText("Documents indexed:")).toHaveTextContent(
      "Documents indexed: 2"
    );
  });
});
