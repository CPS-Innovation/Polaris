import { useMemo } from "react";
import classes from "./UnderRedactionContent.module.scss";
import { RedactionTypes } from "../../../domain/redactionLog/RedactionLogData";
import { ReactComponent as DocIcon } from "../../../../../common/presentation/svgs/doc.svg";
type UnderRedactionContentProps = {
  documentName: string;
  redactionTypes: RedactionTypes[];
};

const getRedactionTypeNames = (count: number, name: string) => {
  if (count <= 1) {
    return name;
  }
  return `${name}s`;
};

export const UnderRedactionContent: React.FC<UnderRedactionContentProps> = ({
  documentName,
  redactionTypes,
}) => {
  const redactionSummary = useMemo(() => {
    const groupedRedactions = redactionTypes.reduce((acc, redactionType) => {
      if (!acc[redactionType?.name!]) {
        acc[redactionType?.name!] = 1;
        return acc;
      }
      acc[redactionType?.name!] = acc[redactionType?.name!] + 1;
      return acc;
    }, {} as Record<string, number>);

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
      <div className={classes.headingWrapper}>
        <DocIcon className={classes.docIcon} />{" "}
        <h2>{`Redaction details for:"${documentName}"`}</h2>
      </div>
      <ul className={classes.underRedactionContentList}>{redactionSummary}</ul>
    </div>
  );
};
