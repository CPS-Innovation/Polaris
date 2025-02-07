import React, { ReactNode } from "react";
import * as GDS from "govuk-react-jsx";

export type BackLinkProps = {
  to: string;
  children?: React.ReactNode;
  label?: ReactNode;
  onClick?: () => void;
};

export const BackLink: React.FC<BackLinkProps> = (props) => {
  return (
    // span to keep the :before arrow glyph and the text of the link vertically aligned together
    <span>
      <GDS.BackLink data-testid="link-back-link" {...props} />
    </span>
  );
};
