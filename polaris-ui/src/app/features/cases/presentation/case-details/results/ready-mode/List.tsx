import { MappedTextSearchResult } from "../../../../domain/MappedTextSearchResult";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import { ListItem } from "./ListItem";

type Props = {
  searchResult: MappedTextSearchResult;
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const List: React.FC<Props> = ({
  searchResult: { documentResults },
  handleOpenPdf,
}) => {
  const visibleResults = documentResults.filter((item) => item.isVisible);

  return (
    <>
      {visibleResults.map((documentResult) => (
        <ListItem
          key={documentResult.documentId}
          documentResult={documentResult}
          handleOpenPdf={handleOpenPdf}
        />
      ))}
    </>
  );
};
