import { Component, ErrorInfo, ReactNode } from "react";
import { Layout } from "../layout/Layout";
import { WaitPage } from ".";
import { CmsAuthRedirectingError } from "../../errors/CmsAuthRedirectingError";
import { ErrorPage } from "./ErrorPage";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | undefined;
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: undefined,
  };

  public static getDerivedStateFromError(error: Error): State {
    // Update state so the next render will show the fallback UI.
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Uncaught error:", error, errorInfo);
  }

  public render() {
    if (this.state.hasError) {
      return (
        <Layout>
          {this.state.error instanceof CmsAuthRedirectingError ? (
            <WaitPage data-testid="div-wait-reauth" />
          ) : (
            <ErrorPage error={this.state.error} />
          )}
        </Layout>
      );
    } else {
      return this.props.children;
    }
  }
}
