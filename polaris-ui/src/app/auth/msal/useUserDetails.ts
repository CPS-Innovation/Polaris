import { UserDetails } from "../UserDetails";
import { msalInstance } from "./msalInstnce";
import { getCookieValue } from "../../common/utils/getCookieValue";

export const useUserDetails = () => {
  const [account] = msalInstance.getAllAccounts();
  const cmsUserID = getCookieValue("UID");

  return {
    name: account.name,
    username: account.username,
    cmsUserID: cmsUserID,
  } as UserDetails;
};
