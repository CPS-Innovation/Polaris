import { useEffect, useState } from "react";
import { ApiError } from "../errors/ApiError";
import { RawApiResult } from "../types/ApiResult";

type UseApiParams = <T extends (...args: any[]) => Promise<any>>(
  del: T,
  params: Parameters<T>,
  makeCall?: boolean
) => RawApiResult<Awaited<ReturnType<typeof del>>>;

/*
  If there is an api method `getFoo(id: number, name: string) => Promise<Model>` then `useApi` is called thus:
    `const state = useApi(getFoo, 1, "bar")`

  The `UseApiParams` type ensures that the second (third.. etc) parameters passed to useApi are
  strongly-typed to the argument types of the function passed as the first param e.g. `getFoo`.

  This approach is borrowed from redux-sagas and avoids the use of anonymous lambdas being passed e.g.

    `const state = useApi(() => getFoo(1, "bar"))`

  meaning that on the inside of `useApi` the function is always seen as being different on every execution
  leading to constant refiring of the `useEffect`.
*/
export const useApi: UseApiParams = (del, params, makeCall = true) => {
  const [result, setResult] = useState<ReturnType<UseApiParams>>({
    status: "initial",
  });

  useEffect(() => {
    if (makeCall) {
      setResult({ status: "loading" });

      del
        .apply(del, params)
        .then((data) => {
          setResult({
            status: "succeeded",
            data,
          });
        })
        .catch((error) =>
          setResult({
            status: "failed",
            error,
            httpStatusCode: error instanceof ApiError ? error.code : undefined,
          })
        );
    }
  }, [del, JSON.stringify(params), makeCall]);

  return result;
};
