import { useMemo } from "react";
import classes from "./UnderRedactionContent.module.scss";

type UnderRedactionContentProps = {
  documentName: string;
};

const getRedactionTypeNames = (count: number, name: string) => {
  if (count <= 1) {
    return name;
  }
  return `${name}s`;
};

export const UnderRedactionContent: React.FC<UnderRedactionContentProps> = ({
  documentName,
}) => {
  const redactionSummary = useMemo(() => {
    const savedRedactions = [
      { redactionType: "Address" },
      { redactionType: "Date of Birth" },
      { redactionType: "Named individual" },
      { redactionType: "Other" },
      { redactionType: "Occupation" },
      { redactionType: "Phone number" },
      { redactionType: "Vehicle registration" },
      { redactionType: "Email address" },
    ];
    const groupedRedactions = savedRedactions.reduce(
      (acc, { redactionType }) => {
        if (!acc[redactionType]) {
          acc[redactionType] = 1;
          return acc;
        }
        acc[redactionType] = acc[redactionType] + 1;
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
