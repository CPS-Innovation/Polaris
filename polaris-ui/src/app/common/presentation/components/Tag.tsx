import * as GDS from "govuk-react-jsx";
import { TagColor } from "../types/TagColor";

export type TagProps = {
  children: React.ReactNode;
  gdsTagColour: TagColor;
  className?: string;
};

export const Tag: React.FC<TagProps> = ({
  className,
  gdsTagColour,
  ...props
}) => {
  return (
    <GDS.Tag
      className={`govuk-tag--${gdsTagColour} ${className || ""}`}
      {...props}
    ></GDS.Tag>
  );
};
