import { useMemo } from "react";
import classes from "./UnderRedactionContent.module.scss";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
type UnderRedactionContentProps = {
  documentName: string;
  redactionHighlights: IPdfHighlight[];
};

const getRedactionTypeNames = (count: number, name: string) => {
  if (count <= 1) {
    return name;
  }
  return `${name}s`;
};

export const UnderRedactionContent: React.FC<UnderRedactionContentProps> = ({
  documentName,
  redactionHighlights,
}) => {
  const redactionSummary = useMemo(() => {
    const groupedRedactions = redactionHighlights.reduce(
      (acc, { redactionType }) => {
        if (!acc[redactionType!]) {
          acc[redactionType!] = 1;
          return acc;
        }
        acc[redactionType!] = acc[redactionType!] + 1;
        return acc;
      },
      {} as Record<string, number>
    );

    const sortedArray = Object.entries(groupedRedactions).sort(function (a, b) {
      return b[1] - a[1];
    });

    return sortedArray.map((item) => (
      <li>
        <b>{`${item[1]}`}</b> - {`${getRedactionTypeNames(item[1], item[0])}`}
      </li>
    ));
  }, []);
  return (
    <div className={classes.underRedactionContent}>
      <h2>{`Redaction details for:${documentName}`}</h2>
      <ul className={classes.underRedactionContentList}>{redactionSummary}</ul>
    </div>
  );
};
