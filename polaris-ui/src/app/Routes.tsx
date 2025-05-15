import { FC, useEffect, useState } from "react";
import { Layout } from "./common/presentation/layout/Layout";
import { Redirect, Route, Switch, useLocation } from "react-router-dom";
import { Helmet, HelmetProvider } from "react-helmet-async";

import CaseSearch, {
  path as caseSearchPath,
} from "./features/cases/presentation/case-search";

import CaseSearchResults, {
  path as caseSearchResultsPath,
} from "./features/cases/presentation/case-search-results";

import Case, {
  path as casePath,
} from "./features/cases/presentation/case-details";
import { testAppInsightsConnection } from "../app/common/utils/appInsightsUtils";
import {
  InboundHandoverHandler,
  inboundHandoverPath,
} from "./inbound-handover/InboundHandoverHandler";
import { isTaggedContext } from "./inbound-handover/context";
import { useUserGroupsFeatureFlag } from "./auth/msal/useUserGroupsFeatureFlag";

export const Routes: FC = () => {
  const { state: navigationState } = useLocation();

  const navigationStateAsContext = isTaggedContext(navigationState)
    ? navigationState
    : undefined;
  const navigationStateAsBacklinkPath =
    typeof navigationState == "string" ? navigationState : undefined;

  const [isAppInsightActive, setIsAppInsightActive] = useState(true);
  useEffect(() => {
    if (
      process.env.NODE_ENV === "production" &&
      process.env.REACT_APP_MOCK_API_SOURCE !== "cypress"
    ) {
      setTimeout(async () => {
        if (!(await testAppInsightsConnection())) {
          setIsAppInsightActive(false);
        }
      }, 1000);
    }
  }, []);

  if (!isAppInsightActive) {
    throw Error("Failed to connect to App Insights");
  }

  return (
    <HelmetProvider>
      <Switch>
        <Route path={caseSearchPath}>
          <Layout>
            <Helmet>
              {" "}
              <title>Casework App case search page</title>
            </Helmet>
            <CaseSearch />
          </Layout>
        </Route>
        <Route path={caseSearchResultsPath}>
          <Layout>
            <Helmet>
              <title>Casework App case search result page</title>
            </Helmet>
            <CaseSearchResults backLinkProps={{ to: caseSearchPath }} />
          </Layout>
        </Route>
        <Route path={casePath}>
          <Layout isWide>
            <Helmet>
              <title>Casework App case details page</title>
            </Helmet>
            <Case
              backLinkProps={{
                // Typically the user will be on the case details page having navigated through the
                //  results screen, and so `state` will be the expected string holding the return
                //  path.  If the user has visited the case screen directly (e.g. bookmarked a case URL)
                //  `state` will be undefined.  In this case, we want to take the user back to the vanilla
                //  home page.  This is best represented by redirecting to empty and allowing the default
                //  route to kick -in.
                to: navigationStateAsBacklinkPath
                  ? caseSearchResultsPath + navigationStateAsBacklinkPath
                  : "/",
                label: "Find a case",
              }}
              context={navigationStateAsContext}
            />
          </Layout>
        </Route>
        <Route
          path={inboundHandoverPath}
          Component={InboundHandoverHandler}
        ></Route>
        <Route>
          <Redirect to={caseSearchPath} />
        </Route>
      </Switch>
    </HelmetProvider>
  );
};
