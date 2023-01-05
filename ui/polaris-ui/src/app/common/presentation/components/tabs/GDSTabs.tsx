/* istanbul ignore file */
import * as GDS from "govuk-react-jsx";
import { CommonTabsProps } from "./types";

export const GDSTabs: React.FC<CommonTabsProps> = (props) => {
  return <GDS.Tabs {...props} />;
};
