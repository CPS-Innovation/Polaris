import * as GDS from "govuk-react-jsx";

type Props = {
  className?: string;
  tag?: {
    children: React.ReactNode;
  };
};
export const NotificationBanner: React.FC<Props> = (props) => (
  <GDS.NotificationBanner {...props} />
);
