import { FetchArgs } from "./core";

export const fetchWithCookies = async (...args: FetchArgs) => {
  return await fetch(args[0], {
    ...args[1],
    // We need cookies to be sent to the gateway, which could be a different domain,
    //  so need to set `credentials: "include"`
    credentials: "include",
  });
};
