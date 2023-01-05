import { UserDetails } from "../UserDetails";
import { msalInstance } from "./msalInstnce";

export const useUserDetails = () => {
  const [account] = msalInstance.getAllAccounts();

  return {
    name: account.name,
    username: account.username,
  } as UserDetails;
};
