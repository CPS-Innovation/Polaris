import { createRoot } from "react-dom/client";
import { App } from "./app/App";

import "./styles.scss";

if (process.env.REACT_APP_MOCK_API_SOURCE === "dev") {
  // only import if we need to, this way production bundles don't contain the mock api code
  import("./mock-api/browser").then(({ setupMockApi }) => {
    setupMockApi({
      baseUrl: process.env.REACT_APP_GATEWAY_BASE_URL ?? "",
      maxDelayMs: process.env.REACT_APP_MOCK_API_MAX_DELAY ?? "0",
      sourceName: "dev",
      publicUrl: process.env.PUBLIC_URL,
      redactionLogUrl: process.env.REACT_APP_REDACTION_LOG_BASE_URL ?? "",
    });
  });
}

const domNode = document.getElementById("root");
const app = createRoot(domNode as HTMLElement);
app.render(
  // <React.StrictMode>
  <div className="govuk-body">
    <App />
  </div>
  // </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
// reportWebVitals();
