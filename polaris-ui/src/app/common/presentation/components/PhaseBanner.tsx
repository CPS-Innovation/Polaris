import * as GDS from "govuk-react-jsx";

type Props = {
  className?: string;
  tag?: {
    children: React.ReactNode;
  };
};
export const PhaseBanner: React.FC<Props> = (props) => (
  <GDS.PhaseBanner {...props} />
);
