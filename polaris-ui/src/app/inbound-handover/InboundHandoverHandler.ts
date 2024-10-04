import { useCallback, useEffect, useState } from "react";
import { lookupUrn } from "../features/cases/api/gateway-api";
import { buildContextFromQueryString, TaggedContext } from "./context";
import { RouteComponentProps } from "react-router-dom";

export const inboundHandoverPath = "/go";

// we should only be rendering if a route has matched to `inboundHandoverPath`
export const InboundHandoverHandler: React.FC<RouteComponentProps> = ({
  location: { search },
  history,
}) => {
  // this allows us to usefully bubble-up an error from within
  //  the useEffect to an ErrorBoundary.
  const [error, setError] = useState<Error>();
  if (error) {
    throw error;
  }

  const getCaseIdentifiers = async (caseId: number) => {
    try {
      const { urnRoot, id } = await lookupUrn(caseId);
      return { caseId: id, urn: urnRoot };
    } catch (ex) {
      setError(ex as Error);
    }
  };

  const navigate = useCallback(
    (caseId: number, urn: string, contextObject: TaggedContext | undefined) =>
      history.push(`/case-details/${urn}/${caseId}`, contextObject),
    [history]
  );

  useEffect(() => {
    (async () => {
      try {
        const { caseId, urn, contextObject } =
          buildContextFromQueryString(search);

        if (urn) {
          // we have not been passed a urn so no need to look up
          navigate(caseId, urn, contextObject);
        } else {
          const response = await getCaseIdentifiers(caseId);
          if (response) {
            navigate(response.caseId, response.urn, contextObject);
          }
        }
      } catch (ex) {
        setError(ex as Error);
      }
    })();
  }, [search, history, navigate]);
  return null;
};
