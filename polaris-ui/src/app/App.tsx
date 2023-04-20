import { FC } from "react";
import { BrowserRouter as Router } from "react-router-dom";
import { Routes } from "./Routes";
import { Auth } from "./auth";
import { ErrorBoundary } from "./common/presentation/components";
import { useHistory } from "react-router-dom";
import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
import { AppInsightsContext } from "@microsoft/applicationinsights-react-js";
export const App: FC = () => {
  const history = useHistory();
  const reactPlugin = new ReactPlugin();
  try {
    // Application Insights Configuration
    const configObj = {
      connectionString:
        "InstrumentationKey=1748cb87-66a6-48f0-9abd-5f00f8ac7bf9;IngestionEndpoint=https://uksouth-0.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/",
      enableAutoRouteTracking: true,
      enableCorsCorrelation: true,
      extensions: [reactPlugin],
      extensionConfig: {
        [reactPlugin.identifier]: { history: history },
      },
    };

    const appInsights = new ApplicationInsights({
      config: configObj,
    });

    appInsights.loadAppInsights();
    appInsights.trackPageView();
  } catch (e) {
    console.log("app insight error", e);
  }
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
