import { UserDetails } from "../UserDetails";

export const useUserDetails = () =>
  ({
    name: "Development User",
    username: "dev_user@example.org",
  } as UserDetails);
