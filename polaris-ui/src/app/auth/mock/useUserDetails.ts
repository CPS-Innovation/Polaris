import { UserDetails } from "../UserDetails";

export const useUserDetails = () =>
  ({
    name: "Development User",
    username: "dev_user@example.org",
    cmsUserID: "dev_mock_cms_id",
  } as UserDetails);
