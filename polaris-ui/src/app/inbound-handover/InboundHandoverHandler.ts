import { useEffect, useState } from "react";
import { lookupUrn } from "../features/cases/api/gateway-api";
import { buildContextFromQueryString, TaggedContext } from "./context";
import { RouteComponentProps } from "react-router-dom";
import { HandoverError } from "../common/errors/HandoverError";

export const inboundHandoverPath = "/go";

// we should only be rendering if a route has matched to `inboundHandoverPath`
export const InboundHandoverHandler: React.FC<RouteComponentProps> = ({
  location: { search },
  history,
}) => {
  const [error, setError] = useState<Error>();
  if (error) {
    throw error;
  }
  useEffect(() => {
    const getUrn = async (
      caseId: number,
      contextObject: TaggedContext | undefined
    ) => {
      try {
        const { urnRoot, id } = await lookupUrn(caseId);
        history.push(`/case-details/${urnRoot}/${id}`, contextObject);
      } catch (ex) {
        setError(ex as Error);
      }
    };

    try {
      const { caseId, urn, contextObject } =
        buildContextFromQueryString(search);
      if (urn) {
        // we have not been passed a urn so no need to look up
        history.push(`/case-details/${urn}/${caseId}`, contextObject);
        return;
      } else {
        getUrn(caseId, contextObject);
      }
    } catch (ex) {
      setError(ex as Error);
    }
  }, [search, history]);
  return null;
};
