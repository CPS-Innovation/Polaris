import React from "react";
import * as GDS from "govuk-react-jsx";

export type TableProps = {
  className?: string;
  rows: {
    cells: {
      children: React.ReactNode;
    }[];
  }[];
};

export const Table: React.FC<TableProps> = (props) => {
  return <GDS.Table {...props}></GDS.Table>;
};
