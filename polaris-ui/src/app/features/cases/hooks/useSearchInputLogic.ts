import { useState, KeyboardEvent } from "react";
import { validateUrn } from "../logic/validate-urn";
import { CaseSearchQueryParams } from "../types/CaseSearchQueryParams";

export const useSearchInputLogic = ({
  urnFromSearchParams,
  setParams,
  search,
}: {
  urnFromSearchParams: string | undefined;
  setParams: (params: Partial<CaseSearchQueryParams>) => void;
  search?: string;
}) => {
  const [urn, setUrn] = useState(urnFromSearchParams || "");
  const [isError, setIsError] = useState(false);

  const allowedParams = ["redactionLog"];
  const queryParams = new URLSearchParams(search);
  const getQueryParamsObject = (queryParams: URLSearchParams) => {
    const params = {} as any;
    for (const [key, value] of queryParams.entries()) {
      if (allowedParams.includes(key)) {
        params[key] = value;
      }
    }
    return params;
  };
  const paramsObject = getQueryParamsObject(queryParams);

  const handleChange = (val: string) => {
    setUrn(val.toUpperCase());
  };

  const handleSubmit = () => {
    const { isValid, rootUrn } = validateUrn(urn);
    setIsError(!isValid);
    if (isValid) {
      // For technical purposes (when forming API URLs) the rootUrn is what we need.
      //  If a user enters e.g. 12AB121212/9, we want to sanitize this
      //  to 12AB121212.  The caseId that is then selected lets us then continue
      //  with the correct split case.
      setParams({ ...paramsObject, urn: rootUrn });
    }
  };

  const handleKeyPress = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleSubmit();
    }
  };

  return {
    handleChange,
    handleKeyPress,
    handleSubmit,
    urn,
    isError,
  };
};
