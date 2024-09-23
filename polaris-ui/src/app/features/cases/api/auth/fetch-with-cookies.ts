export const fetchWithCookies = async (...args: Parameters<typeof fetch>) => {
  return await fetch(args[0], {
    ...args[1],
    // We need cookies to be sent to the gateway, which could be a different domain,
    //  so need to set `credentials: "include"`
    credentials: "include",
  });
};
