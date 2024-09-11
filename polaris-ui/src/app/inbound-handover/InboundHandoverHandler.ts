import { useEffect } from "react";
import { lookupUrn } from "../features/cases/api/gateway-api";
import { buildContextFromQueryString } from "./context";
import { RouteComponentProps } from "react-router-dom";
import { HandoverError } from "../common/errors/HandoverError";

export const inboundHandoverPath = "/go";

// we should only be rendering if a route has matched to `inboundHandoverPath`
export const InboundHandoverHandler: React.FC<RouteComponentProps> = ({
  location: { search },
  history,
}) => {
  const { caseId, urn, contextObject } = buildContextFromQueryString(search);

  useEffect(() => {
    const getUrn = async () => {
      let urnToUse = urn;
      let caseIdToUse = caseId;
      if (!urnToUse) {
        // we have not been passed a urn in the context and so we need ot go and look up one
        try {
          const { urnRoot, id } = await lookupUrn(caseId);
          urnToUse = urnRoot;
          caseIdToUse = id;
        } catch (ex) {
          throw new HandoverError(
            "Could not look-up urn as part of context handover:",
            (ex as Error).message
          );
        }
      }
      history.push(`/case-details/${urnToUse}/${caseIdToUse}`, contextObject);
    };

    getUrn();
  }, [caseId, urn, contextObject, history]);
  return null;
};
