import { FetchArgs } from "./core";

/**
 * Just adds the credentials: "include" flag to fetch options
 *
 * We need cookies to be sent to the gateway, so need to set `credentials: "include"`
 * - consider this "defaultFetch"
 */
export const fetchWithCookies = async (...args: FetchArgs) => {
  return await fetch(args[0], {
    ...args[1],
    credentials: "include",
  });
};
