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

  const navigateToCase = useCallback(
    (
      caseId: number,
      urn: string,
      contextObject: TaggedContext | undefined,
      contextSearchParams: string
    ) =>
      history.push(
        `/case-details/${urn}/${caseId}${contextSearchParams}`,
        contextObject
      ),
    [history]
  );

  const navigateToDocument = useCallback(
    (caseId: number, urn: string, documentId: number) =>
      history.push(`/case-details/${urn}/${caseId}/CMS-${documentId}/#dcf`),
    [history]
  );

  useEffect(() => {
    (async () => {
      try {
        const { caseId, urn, documentId, contextObject, contextSearchParams } =
          buildContextFromQueryString(search);

        if (documentId) {
          navigateToDocument(caseId, urn!, documentId);
        } else {
          if (urn) {
            navigateToCase(caseId, urn, contextObject, contextSearchParams);
          } else {
            // we have not been passed a urn so no need to look up
            const response = await getCaseIdentifiers(caseId);
            if (response) {
              navigateToCase(
                response.caseId,
                response.urn,
                contextObject,
                contextSearchParams
              );
            }
          }
        }
      } catch (ex) {
        setError(ex as Error);
      }
    })();
  }, [search, history, navigateToCase, navigateToDocument]);

  return null;
};
