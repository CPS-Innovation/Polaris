import { useHistory, useLocation } from "react-router-dom";
import { parse, stringify } from "qs";
import { path } from "../../features/cases/presentation/case-search-results";

export type QueryParamsState<T> = T & {
  setParams: (params: Partial<T>) => void;
  search: string;
};

export const useQueryParamsState = <T>(): QueryParamsState<T> => {
  const { search } = useLocation();
  const { push } = useHistory();

  const params = parse(search, {
    ignoreQueryPrefix: true,
    comma: true,
  }) as unknown as T;

  const setParams = (params: Partial<T>) => {
    const queryString = stringify(params, {
      addQueryPrefix: true,
      encode: false,
      arrayFormat: "comma",
    });
    push(`${path}${queryString}`);
  };

  return {
    setParams,
    search,
    ...params,
  };
};
