import * as GDS from "govuk-react-jsx";

export type TagProps = {
  children: React.ReactNode;
  className?: string;
};

export const Tag: React.FC<TagProps> = (props) => {
  return <GDS.Tag {...props}></GDS.Tag>;
};
