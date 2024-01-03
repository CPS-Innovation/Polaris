import {
  InteractionRequiredAuthError,
  SilentRequest,
} from "@azure/msal-browser";
import { msalInstance } from "./msalInstance";
import { Client } from "@microsoft/microsoft-graph-client";

const getUserRolesAndAreaDivisions = async (client: Client): Promise<any[]> => {
  let userAreaDivisions: any[] = [];

  let userGroups = await GetGroups(client, "/me/transitiveMemberOf");
  AddRedactionLogGroups(userGroups.value, userAreaDivisions);

  while (userGroups["@odata.nextLink"]) {
    userGroups = await GetGroups(client, userGroups["@odata.nextLink"]);
    AddRedactionLogGroups(userGroups.value, userAreaDivisions);
  }

  return userAreaDivisions;
};

const AddRedactionLogGroups = (
  groups: { displayName: string }[],
  userAreaDivisions: any[]
) => {
  groups.forEach((ug) => {
    if (ug?.displayName) {
      const displayName = ug.displayName.toUpperCase();
      userAreaDivisions.push(displayName);
      // if (Object.keys(areaDivisionsByGroup).includes(displayName)) {
      //     userAreaDivisions.push(areaDivisionsByGroup[displayName]);
      // }
    }
  });
};

const GetGroups = async (
  client: Client,
  nextLink: string
): Promise<{
  value: { displayName: string }[];
  "@odata.nextLink": string;
}> => {
  return await client.api(nextLink).select("displayName").get();
};

export const getAccessToken = async (scopes: string[]) => {
  const [account] = msalInstance.getAllAccounts();

  const request = {
    scopes,
    account,
  } as SilentRequest;

  try {
    const { accessToken } = await msalInstance.acquireTokenSilent(request);
    const client = Client.init({
      authProvider: (done: any) => {
        done(null, accessToken);
      },
    });
    try {
      const areaDivisions = await getUserRolesAndAreaDivisions(client);
    } catch (e) {}

    return accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect(request);
    }
    return String(); // so the return type is always string
  }
};
