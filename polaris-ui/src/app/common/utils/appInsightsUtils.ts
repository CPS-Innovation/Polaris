import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
export const initializeAppInsights = () => {
  /*
  AppInsights is initialized based on the valid connection string passed in through env variable
  REACT_APP_AI_CONNECTION_STRING. This variable is set to empty for cypress test and local .env file. 
  If we ever want to test appinsight locally, please pass in the connection string from "ai-polaris-dev"(check in the azure portal) 
  in the local env file.
  */

  const reactPlugin = new ReactPlugin();
  try {
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
