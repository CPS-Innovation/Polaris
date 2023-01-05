import { rest } from "msw";
import { setupServer } from "msw/node";
import dataSource from "../../../mock-api/data/searchResults.integration";
import { GATEWAY_BASE_URL } from "../../config";
import { render as tlRender, fireEvent, waitFor } from "@testing-library/react";
import { Page } from "./presentation/search/Page";
import { Router } from "react-router-dom";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import reducer from "./redux/casesSlice";
import { FC } from "react";
import { createMemoryHistory, MemoryHistory } from "history";

const fullPath = (path: string) => new URL(path, GATEWAY_BASE_URL).toString();

const server = setupServer(
  rest.get(fullPath("/cases/search/"), (req, res, ctx) => {
    const urn = req.url.searchParams.get("urn")!;
    return res(ctx.json(dataSource(urn)));
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

type RenderParams = Parameters<typeof tlRender>;
const render = (
  ui: RenderParams[0],
  {
    history,
    preloadedState,
    ...rest
  }: RenderParams[1] & {
    history: MemoryHistory<unknown>;
    preloadedState?: Parameters<typeof configureStore>[0]["preloadedState"];
  }
) => {
  const store = configureStore({
    reducer: { cases: reducer },
    preloadedState: preloadedState || {},
  });

  const Wrapper: FC = ({ children }) => (
    <Provider store={store}>
      <Router history={history}>{children}</Router>
    </Provider>
  );

  return tlRender(ui, { wrapper: Wrapper, ...rest });
};

describe("Case search", () => {
  test("can return cases for a given URN", async () => {
    const history = createMemoryHistory();
    const { getByTestId, getAllByTestId } = render(<Page />, { history });
    const urnInput = getByTestId("input-search-urn") as HTMLInputElement;
    const searchBtn = getByTestId("button-search") as HTMLButtonElement;

    // default state of dom
    expect(urnInput).toBeInTheDocument();
    expect(urnInput.value).toBe("");
    expect(searchBtn).toBeInTheDocument();
    expect(searchBtn.disabled).toBe(true);

    // enter a urn ...
    fireEvent.change(urnInput, { target: { value: "12AB1111111" } });

    /// ... and should be able to click button
    expect(searchBtn.disabled).toBe(false);
    fireEvent.click(searchBtn);
    expect(history.location.search).toBe("?urn=12AB1111111");

    await waitFor(() =>
      expect(getByTestId("element-results")).toBeInTheDocument()
    );

    expect(getAllByTestId("element-result").length).toBe(1);

    // can find another urn results
    fireEvent.change(urnInput, { target: { value: "12AB2222222" } });
    fireEvent.click(searchBtn);

    expect(history.location.search).toBe("?urn=12AB2222222");

    // can search by pressing enter
    fireEvent.change(urnInput, { target: { value: "12AB3333333" } });
    fireEvent.click(searchBtn);
    fireEvent.keyDown(urnInput, { key: "Enter" });
    expect(history.location.search).toBe("?urn=12AB3333333");

    await waitFor(() =>
      expect(getAllByTestId("element-result").length).toBe(2)
    );
  });
});
