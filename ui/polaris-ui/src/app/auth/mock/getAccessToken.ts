export const getAccessToken = (scopes: string[]) =>
  Promise.resolve(`mock_token for scopes: ${scopes.join(",")}`);
