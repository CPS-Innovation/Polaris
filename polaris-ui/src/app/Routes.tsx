import { FC } from "react";
import { Layout } from "./common/presentation/layout/Layout";
import { Redirect, Route, Switch } from "react-router-dom";

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
  return (
    <Switch>
      <Route path={caseSearchPath}>
        <Layout>
          <CaseSearch />
        </Layout>
      </Route>
      <Route path={caseSearchResultsPath}>
        <Layout>
          <CaseSearchResults />
        </Layout>
      </Route>
      <Route path={casePath}>
        <Layout isWide>
          <Case />
        </Layout>
      </Route>
      <Route>
        <Redirect to={caseSearchPath} />
      </Route>
    </Switch>
  );
};
