import { useMemo } from "react";
import classes from "./UnderRedactionContent.module.scss";
import { RedactionTypeData } from "../../../domain/redactionLog/RedactionLogData";

type UnderRedactionContentProps = {
  documentName: string;
  savedRedactionTypes: RedactionTypeData[];
};

const getRedactionTypeNames = (count: number, name: string) => {
  if (count <= 1) {
    return name;
  }
  return `${name}s`;
};

export const UnderRedactionContent: React.FC<UnderRedactionContentProps> = ({
  documentName,
  savedRedactionTypes,
}) => {
  const redactionSummary = useMemo(() => {
    const groupedRedactions = savedRedactionTypes.reduce(
      (acc, redactionType) => {
        if (!acc[redactionType?.name!]) {
          acc[redactionType?.name!] = 1;
          return acc;
        }
        acc[redactionType?.name!] = acc[redactionType?.name!] + 1;
        return acc;
      },
      {} as Record<string, number>
    );

    const sortedArray = Object.entries(groupedRedactions).sort(function (a, b) {
      return b[1] - a[1];
    });

    return sortedArray.map((item) => (
      <li key={`${item[0]}`}>
        <b>{`${item[1]}`}</b> - {`${getRedactionTypeNames(item[1], item[0])}`}
      </li>
    ));
  }, []);
  return (
    <div className={classes.underRedactionContent}>
      <ul
        className={classes.underRedactionContentList}
        data-testid="redaction-summary"
      >
        {redactionSummary}
      </ul>
    </div>
  );
};
