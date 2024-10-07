import { UserDetails } from "../UserDetails";
import { msalInstance } from "./msalInstance";
import { getCookieValue } from "../../common/utils/getCookieValue";
import { useMemo } from "react";

export const useUserDetails = () => {
  const [{ name, username }] = msalInstance.getAllAccounts();

  const cmsUserID = getCookieValue("UID");

  const userDetails = useMemo(
    () =>
      ({
        name,
        username,
        cmsUserID,
      } as UserDetails),
    [name, username, cmsUserID]
  );

  return userDetails;
};
