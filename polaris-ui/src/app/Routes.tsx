import { FC } from "react";
import { Layout } from "./common/presentation/layout/Layout";
import { Redirect, Route, Switch, useLocation } from "react-router-dom";

import CaseSearch, {
  path as caseSearchPath,
} from "./features/cases/presentation/case-search";

import CaseSearchResults, {
  path as caseSearchResultsPath,
} from "./features/cases/presentation/case-search-results";

import Case, {
  path as casePath,
} from "./features/cases/presentation/case-details";

export const Routes: FC = () => {
  const { state } = useLocation();

  return (
    <Switch>
      <Route path={caseSearchPath}>
        <Layout>
          <CaseSearch />
        </Layout>
      </Route>
      <Route path={caseSearchResultsPath}>
        <Layout>
          <CaseSearchResults backLinkProps={{ to: caseSearchPath }} />
        </Layout>
      </Route>
      <Route path={casePath}>
        <Layout isWide>
          <Case
            backLinkProps={{
              // Typically the user will be on the case details page having navigated through the
              //  results screen, and so `state` will be the expected string holding the return
              //  path.  If the user has visited the case screen directly (e.g. bookmarked a case URL)
              //  `state` will be undefined.  In this case, we want to take the user back to the vanilla
              //  home page.  This is best represented by redirecting to empty and allowing the default
              //  route to kick -in.
              to: state ? caseSearchResultsPath + state : "/",
              label: "Find a case",
            }}
          />
        </Layout>
      </Route>
      <Route>
        <Redirect to={caseSearchPath} />
      </Route>
    </Switch>
  );
};
