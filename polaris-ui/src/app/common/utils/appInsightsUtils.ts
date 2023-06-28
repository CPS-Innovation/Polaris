import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
import { AI_CONNECTION_STRING } from "../../config";

let appInsights: ApplicationInsights | null = null;
/*
  AppInsights is initialized based on the valid connection string passed in through env variable
  REACT_APP_AI_CONNECTION_STRING. This variable is set to empty for cypress test and local .env file. 
  If we ever want to test appinsight locally, please pass in the connection string from "ai-polaris-dev"(check in the azure portal) 
  in the local env file.
*/

export const initializeAppInsights = () => {
  const reactPlugin = new ReactPlugin();
  try {
    const configObj = {
      connectionString: AI_CONNECTION_STRING,
      enableCorsCorrelation: true,
      extensions: [reactPlugin],
    };
    appInsights = new ApplicationInsights({
      config: configObj,
    });

    appInsights.loadAppInsights();
  } catch (e) {
    console.log("app insight error", e);
  }
  return reactPlugin;
};

export const getAppInsights = () => appInsights;

export const testAppInsightsConnection = async () => {
  const appInsights = getAppInsights();
  if (!appInsights) {
    return false;
  }

  const endpointUrl = appInsights.config.endpointUrl;
  const instrumentationKey = appInsights.config.instrumentationKey;
  if (!endpointUrl || !instrumentationKey) {
    return false;
  }

  try {
    const response = await fetch(`${endpointUrl}`, {
      method: "POST",
      body: JSON.stringify([
        {
          data: {
            baseData: { name: "ai-test-event", properties: {} },
            baseType: "EventData",
          },
          iKey: instrumentationKey,
          name: `Microsoft.ApplicationInsights.${instrumentationKey}.Event`,
          time: new Date(),
        },
      ]),
    });

    const result = await response.json();
    if (result.errors.length) {
      console.error("AppInsight Error", result.errors);
      return false;
    }
  } catch (e) {
    return false;
  }
  return true;
};
