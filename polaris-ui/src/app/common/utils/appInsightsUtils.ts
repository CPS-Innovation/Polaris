import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
import { AI_CONNECTION_STRING } from "../../config";

/*
  AppInsights is initialized based on the valid connection string passed in through env variable
  REACT_APP_AI_CONNECTION_STRING. This variable is set to empty for cypress test and local .env file. 
  If we ever want to test appinsight locally, please pass in the connection string from "ai-polaris-dev"(check in the azure portal) 
  in the local env file.
  */

const reactPlugin = new ReactPlugin();

const configObj = {
  connectionString: AI_CONNECTION_STRING,
  enableCorsCorrelation: true,
  extensions: [reactPlugin],
};
const appInsights = new ApplicationInsights({
  config: configObj,
});

console.log("ApplicationInsights>>", appInsights.config);
try {
  appInsights.loadAppInsights();
} catch (e) {
  console.log("app insight error", e);
}
export { reactPlugin };

// const getKeyValue = (inputString: string | undefined, key: string) => {
//   if (!inputString) return "";
//   const keyRegex = new RegExp(`${key}=(.*?);`);
//   const keyMatch = inputString.match(keyRegex);
//   return keyMatch ? keyMatch[1] : "";
// };

export const testAppInsightsConnection = async () => {
  const endpointUrl = appInsights.config.endpointUrl;
  const instrumentationKey = appInsights.config.instrumentationKey;
  console.log("endpointUrl>>", endpointUrl);
  console.log("instrumentationKey>>", instrumentationKey);
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
