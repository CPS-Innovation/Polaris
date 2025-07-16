import { Tabs, TabsProps } from "./Tabs";
import { render, screen, fireEvent } from "@testing-library/react";
import { useState } from "react";
import { within } from "@testing-library/dom";
jest.mock("../../../../common/hooks/useAppInsightsTracks", () => ({
  useAppInsightsTrackEvent: () => jest.fn(),
}));

describe("Tabs", () => {
  const scrollIntoView = Element.prototype.scrollIntoView;

  beforeAll(() => {
    Element.prototype.scrollIntoView = jest.fn();
  });

  afterAll(() => {
    Element.prototype.scrollIntoView = scrollIntoView;
  });

  it("can render empty tabs", async () => {
    const props: TabsProps = {
      idPrefix: "foo",
      title: "Tabs-title",
      items: [],
      activeTabId: "",
      handleClosePdf: () => {},
      handleTabSelection: () => {},
      handleUnLockDocuments: () => {},
      dcfMode: "dcf",
    };

    render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(0);
  });

  it("can render tabs", async () => {
    const props: TabsProps = {
      idPrefix: "foo",
      title: "Tabs-title",
      activeTabId: "",
      items: [
        {
          id: "t1",
          versionId: 1,
          label: "tab-1",
          panel: <></>,
          isDirty: false,
        },
        {
          id: "t2",
          versionId: 1,
          label: "tab-2",
          panel: <></>,
          isDirty: false,
        },
        {
          id: "t3",
          versionId: 1,
          label: "tab-3",
          panel: <></>,
          isDirty: false,
        },
      ],
      handleClosePdf: () => {},
      handleTabSelection: () => {},
      handleUnLockDocuments: () => {},
      dcfMode: "dcf",
    };

    render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(3);
  });

  it("can highlight the active tab", async () => {
    const chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    let keyStr = "";
    for (let i = 0; i < 10; i++) {
      keyStr += chars[Math.floor(Math.random() * chars.length)];
    }

    const props: TabsProps = {
      key: `${keyStr}`,
      idPrefix: "foo",
      title: "Tabs-title",
      activeTabId: "",
      dcfMode: "dcf",
      items: [
        {
          id: "t1",
          versionId: 1,
          label: "tab-1",
          panel: <>content-1</>,
          isDirty: false,
        },
        {
          id: "t2",
          versionId: 1,
          label: "tab-2",
          panel: <>content-2</>,
          isDirty: false,
        },
        {
          id: "t3",
          versionId: 1,
          label: "tab-3",
          panel: <>content-3</>,
          isDirty: false,
        },
      ],
      handleClosePdf: () => {},
      handleTabSelection: () => {},
      handleUnLockDocuments: () => {},
    };

    const { rerender } = render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    // first tab is active if no hash passed
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    expect(screen.getByTestId("tab-content-t1")).not.toHaveClass(
      "hideTabDocument"
    );
    expect(screen.getByTestId("tab-content-t2")).toHaveClass("hideTabDocument");
    rerender(<Tabs {...props} activeTabId="t2" />);

    // otherwise active tab driven by hash
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    expect(screen.getByTestId("tab-content-t1")).toHaveClass("hideTabDocument");
    expect(screen.getByTestId("tab-content-t2")).not.toHaveClass(
      "hideTabDocument"
    );
  });

  it("can navigate using keyboard", async () => {
    const TestComponent = () => {
      const [activeTabId, setActiveTabId] = useState("");
      const props: TabsProps = {
        idPrefix: "foo",
        activeTabId,
        title: "Tabs-title",
        dcfMode: undefined,
        items: [
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t2",
            versionId: 1,
            label: "tab-2",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t3",
            versionId: 1,
            label: "tab-3",
            panel: <></>,
            isDirty: false,
          },
        ],
        handleClosePdf: () => {},
        handleTabSelection: (id: string) => {
          setActiveTabId(id);
        },
        handleUnLockDocuments: () => {},
      };
      return (
        <div>
          <Tabs {...props} />
        </div>
      );
    };

    render(<TestComponent />);
    await screen.findByTestId("tabs");
    // make sure we have landed on the expected first tab
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    // right goes to next tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    // down goes to next tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-3");
    // stays on right-most tab on attempted navigate
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowRight",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-3");
    // left goes to previous tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-2");
    // up goes to previous tab
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    // stays on left-most tab on attempted navigate
    fireEvent.keyDown(screen.getByTestId("tab-active"), {
      code: "ArrowLeft",
    });
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
  });

  // // Not strictly a testable feature as the tab code just renders what it gets given in terms of items.
  // //  However, (one of) the reasons we writing our own tabs is that adding new tabs dynamically
  // //  breaks standard GDS tabs (the keyboard navigation doesn't work for the newly added tabs).
  // //  So lets just check that we are achieving our goal.
  it("can add a tab", async () => {
    const props: TabsProps = {
      idPrefix: "foo",
      title: "Tabs-title",
      activeTabId: "",
      handleClosePdf: () => {},
      handleTabSelection: () => {},
      handleUnLockDocuments: () => {},
      dcfMode: undefined,
      items: [],
    };

    const { rerender } = render(<Tabs {...props} />);
    await screen.findByTestId("tabs");
    expect(screen.queryAllByRole("tab")).toHaveLength(0);
    rerender(
      <Tabs
        {...props}
        items={[
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
        ]}
      />
    );
    expect(screen.queryAllByRole("tab")).toHaveLength(2);
    // going from no tabs to one tab we expect the new tab to get focus
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
    rerender(
      <Tabs
        {...props}
        items={[
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t2",
            versionId: 1,
            label: "tab-2",
            panel: <></>,
            isDirty: false,
          },
        ]}
      />
    );
    expect(screen.queryAllByRole("tab")).toHaveLength(3);
    // going from some tabs to one more tab we expect the new tab NOT to get focus
    expect(screen.getByTestId("tab-active")).toHaveTextContent("tab-1");
  });

  describe("removing tabs", () => {
    it("can remove a tab and trigger navigation to the previous tab", async () => {
      const mockHandleClosePdf = jest.fn();
      const mockHandleTabSelection = jest.fn();
      const props: TabsProps = {
        idPrefix: "foo",
        title: "Tabs-title",
        activeTabId: "t2",
        dcfMode: undefined,
        items: [
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t2",
            versionId: 1,
            label: "tab-2",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t3",
            versionId: 1,
            label: "tab-3",
            panel: <></>,
            isDirty: false,
          },
        ],
        handleClosePdf: mockHandleClosePdf,
        handleTabSelection: mockHandleTabSelection,
        handleUnLockDocuments: () => {},
      };
      render(<Tabs {...props} />);
      await screen.findByTestId("tabs");
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(0);
      const secondTab = screen.getByTestId("tab-1");
      expect(within(secondTab).getByTestId("tab-active")).toHaveTextContent(
        "tab-2"
      );
      fireEvent(
        within(secondTab).getByTestId("tab-remove"),
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
        })
      );
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(1);
      expect(mockHandleClosePdf).toHaveBeenCalledWith("t2", 1);
      expect(mockHandleTabSelection).toHaveBeenCalledTimes(1);
      expect(mockHandleTabSelection).toHaveBeenCalledWith("t1");
    });

    it("can remove the first tab and trigger navigation to the next tab", async () => {
      const mockHandleClosePdf = jest.fn();
      const mockHandleTabSelection = jest.fn();
      const props: TabsProps = {
        idPrefix: "foo",
        title: "Tabs-title",
        activeTabId: "t1",
        dcfMode: undefined,
        items: [
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t2",
            versionId: 1,
            label: "tab-2",
            panel: <></>,
            isDirty: false,
          },
          {
            id: "t3",
            versionId: 1,
            label: "tab-3",
            panel: <></>,
            isDirty: false,
          },
        ],
        handleClosePdf: mockHandleClosePdf,
        handleTabSelection: mockHandleTabSelection,
        handleUnLockDocuments: () => {},
      };
      render(<Tabs {...props} />);
      await screen.findByTestId("tabs");
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(0);
      const fistTab = screen.getByTestId("tab-0");
      expect(within(fistTab).getByTestId("tab-active")).toHaveTextContent(
        "tab-1"
      );
      fireEvent(
        within(fistTab).getByTestId("tab-remove"),
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
        })
      );
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(1);
      expect(mockHandleClosePdf).toHaveBeenCalledWith("t1", 1);
      expect(mockHandleTabSelection).toHaveBeenCalledTimes(1);
      expect(mockHandleTabSelection).toHaveBeenCalledWith("t2");
    });

    it("can remove the only tab and trigger navigation to empty hash", async () => {
      const mockHandleClosePdf = jest.fn();
      const mockHandleTabSelection = jest.fn();
      const props: TabsProps = {
        idPrefix: "foo",
        title: "Tabs-title",
        activeTabId: "t1",
        dcfMode: undefined,
        items: [
          {
            id: "t1",
            versionId: 1,
            label: "tab-1",
            panel: <></>,
            isDirty: false,
          },
        ],
        handleClosePdf: mockHandleClosePdf,
        handleTabSelection: mockHandleTabSelection,
        handleUnLockDocuments: () => {},
      };
      render(<Tabs {...props} />);
      await screen.findByTestId("tabs");
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(0);
      const fistTab = screen.getByTestId("tab-0");
      expect(within(fistTab).getByTestId("tab-active")).toHaveTextContent(
        "tab-1"
      );
      fireEvent(
        within(fistTab).getByTestId("tab-remove"),
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
        })
      );
      expect(mockHandleClosePdf).toHaveBeenCalledTimes(1);
      expect(mockHandleClosePdf).toHaveBeenCalledWith("t1", 1);
      expect(mockHandleTabSelection).toHaveBeenCalledTimes(1);
      expect(mockHandleTabSelection).toHaveBeenCalledWith("");
    });
  });
});
