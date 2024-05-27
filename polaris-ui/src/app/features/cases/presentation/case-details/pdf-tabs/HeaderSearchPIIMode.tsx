import { useMemo } from "react";
import { ISearchPIIHighlight } from "../../../../cases/domain/NewPdfHighlight";
import classes from "./HeaderSearchPIIMode.module.scss";

type Props = {
  searchPIIHighlights: ISearchPIIHighlight[];
};

export const HeaderSearchPIIMode: React.FC<Props> = ({
  searchPIIHighlights,
}) => {
  const groupByRedactionType = useMemo(() => {
    const redactionTypeGroups =
      searchPIIHighlights.reduce((acc, highlight) => {
        const { name } = highlight.redactionType;
        if (!acc[`${name}`]) {
          acc[`${name}`] = 1;
          return acc;
        }
        acc[`${name}`] = acc[`${name}`] + 1;
        return acc;
      }, {} as Record<string, number>) ?? {};
    const entries = Object.entries(redactionTypeGroups);
    entries.sort((a, b) => b[1] - a[1]);
    const sortedRedactionTypeGroups = Object.fromEntries(entries);
    return sortedRedactionTypeGroups;
  }, [searchPIIHighlights]);
  return (
    <div className={classes.headerSearchPIIMode}>
      <h4 className={classes.title}>Potential redactions</h4>
      <span>
        The following terms are items that could potentially be redacted in this
        document:{" "}
      </span>
      <ul className={classes.pIITypesList}>
        {Object.entries(groupByRedactionType).map((keyValue, index) => {
          return (
            <li key={keyValue[0]} className={classes.pIITypesListItem}>
              {index < Object.entries(groupByRedactionType).length - 1 ? (
                <>
                  <span
                    className={classes.redactionTypeCount}
                  >{`(${keyValue[1]}) `}</span>
                  <span>{`${keyValue[0]},`}</span>
                </>
              ) : (
                <>
                  <span
                    className={classes.redactionTypeCount}
                  >{`(${keyValue[1]}) `}</span>
                  <span>{`${keyValue[0]}`}</span>
                </>
              )}
            </li>
          );
        })}
      </ul>
      <span className={classes.attachmentCountText}>{}</span>
    </div>
  );
};
