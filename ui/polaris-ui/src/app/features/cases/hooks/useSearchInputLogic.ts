import { useState, KeyboardEvent } from "react";
import { isUrnValid } from "../logic/is-urn-valid";
import { CaseSearchQueryParams } from "../types/CaseSearchQueryParams";

export const useSearchInputLogic = ({
  initialUrn,
  setParams,
}: {
  initialUrn: string | undefined;
  setParams: (params: Partial<CaseSearchQueryParams>) => void;
}) => {
  const [urn, setUrn] = useState(initialUrn || "");
  const [isError, setIsError] = useState(false);

  const handleChange = (val: string) => {
    setUrn(val.toUpperCase());
  };

  const handleSubmit = () => {
    const isValid = isUrnValid(urn);
    setIsError(!isValid);
    if (isValid) {
      setParams({ urn });
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
