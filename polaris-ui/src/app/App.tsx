import { FC } from "react";
import { BrowserRouter as Router } from "react-router-dom";
import { Routes } from "./Routes";
import { Auth } from "./auth";
import { ErrorBoundary } from "./common/presentation/components";
import { AppInsightsContext } from "@microsoft/applicationinsights-react-js";
import { initializeAppInsights } from "../app/common/utils/appInsightsUtils";
import { clearDownStorage } from "../app/features/cases/presentation/case-details/utils/localStorageUtils";
export const App: FC = () => {
  const reactPlugin = initializeAppInsights();
  clearDownStorage();

  return (
    <AppInsightsContext.Provider value={reactPlugin}>
      <ErrorBoundary>
        <Auth>
          <Router basename={process.env.PUBLIC_URL}>
            <Routes />
          </Router>
        </Auth>
      </ErrorBoundary>
    </AppInsightsContext.Provider>
  );
};
