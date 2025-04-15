import { CombinedState } from "../../../../domain/CombinedState";
import { FeatureFlagData } from "../../../../domain/FeatureFlagData";
import { MappedTextSearchResult } from "../../../../domain/MappedTextSearchResult";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import { ListItem } from "./ListItem";

type Props = {
  searchResult: MappedTextSearchResult;
  submittedSearchTerm: CombinedState["searchState"]["submittedSearchTerm"];
  featureFlags: FeatureFlagData;
  handleOpenPdf: CaseDetailsState["handleOpenPdf"];
};

export const List: React.FC<Props> = ({
  searchResult: { documentResults },
  submittedSearchTerm,
  featureFlags,
  handleOpenPdf,
}) => {
  const visibleResults = documentResults.filter((item) => item.isVisible);

  return (
    <>
      {visibleResults.map((documentResult) => (
        <ListItem
          key={documentResult.documentId}
          documentResult={documentResult}
          submittedSearchTerm={submittedSearchTerm}
          featureFlags={featureFlags}
          handleOpenPdf={handleOpenPdf}
        />
      ))}
    </>
  );
};
