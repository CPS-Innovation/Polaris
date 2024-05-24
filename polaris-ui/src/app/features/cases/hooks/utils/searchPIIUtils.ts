import { ISearchPIIHighlight } from "../../domain/NewPdfHighlight";

export const getRedactionSuggestionTextGroupedByGroupId = (
  searchPIIHighlights: ISearchPIIHighlight[]
) => {
  const groupedTextByGroupId = searchPIIHighlights.reduce((acc, highlight) => {
    if (highlight.redactionStatus !== "redacted") {
      return acc;
    }
    if (!acc[`${highlight.groupId}`]) {
      acc[`${highlight.groupId}`] = highlight.textContent;
      return acc;
    }

    acc[`${highlight.groupId}`] = `${acc[`${highlight.groupId}`]} ${
      highlight.textContent
    }`;
    return acc;
  }, {} as Record<string, string>);

  return groupedTextByGroupId;
};
