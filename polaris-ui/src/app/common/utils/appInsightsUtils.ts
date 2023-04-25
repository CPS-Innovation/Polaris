import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
export const initialiseAppInsights = () => {
  const reactPlugin = new ReactPlugin();
  try {
    // Application Insights Configuration
    const configObj = {
      connectionString: process.env.REACT_APP_AI_CONNECTION_STRING,
      enableCorsCorrelation: true,
      extensions: [reactPlugin],
    };
    const appInsights = new ApplicationInsights({
      config: configObj,
    });

    appInsights.loadAppInsights();
  } catch (e) {
    console.log("app insight error", e);
  }

  return reactPlugin;
};
