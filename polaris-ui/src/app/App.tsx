import { FC } from "react";
import { BrowserRouter as Router } from "react-router-dom";
import { Routes } from "./Routes";
import { Auth } from "./auth";
import { ErrorBoundary } from "./common/presentation/components";

export const App: FC = () => {
  return (
    <>
      <ErrorBoundary>
        <Auth>
          <Router basename={process.env.PUBLIC_URL}>
            <Routes />
          </Router>
        </Auth>
      </ErrorBoundary>
    </>
  );
};
