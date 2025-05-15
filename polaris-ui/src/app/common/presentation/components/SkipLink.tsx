import * as GDS from "govuk-react-jsx";

type Props = {
  href?: string;
  children: React.ReactNode;
};
export const SkipLink: React.FC<Props> = (props) => <GDS.SkipLink {...props} />;
