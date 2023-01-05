import { GATEWAY_BASE_URL } from "../../../config";

export const fullUrl = (path: string) => {
  return new URL(path, GATEWAY_BASE_URL).toString();
};

export const buildFullUrl = <T extends Record<string, string | number>>(
  arg: T,
  del: (processedArg: T) => string
) => {
  const safeArg = Object.entries(arg).reduce(
    (prev, curr) => ({ ...prev, [curr[0]]: encodeURIComponent(curr[1]) }),
    {} as Partial<T>
  ) as T;

  const resolvedPath = del(safeArg);
  return fullUrl(resolvedPath);
};
