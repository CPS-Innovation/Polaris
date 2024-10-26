import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import classes from "./HeaderSearchMode.module.scss";
import { HeaderSearchModeNavigation } from "./HeaderSearchModeNavigation";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "search" }>;
  handleLaunchSearchResults: () => void;
  focussedHighlightIndex: number;
  handleSetFocussedHighlightIndex: (nextIndex: number) => void;
};

export const HeaderSearchMode: React.FC<Props> = ({
  caseDocumentViewModel: {
    searchTerm,
    occurrencesInDocumentCount,
    presentationTitle,
  },
  focussedHighlightIndex,
  handleSetFocussedHighlightIndex,
  handleLaunchSearchResults,
}) => {
  return (
    <div className={classes.content}>
      <div className={classes.heavyText}>
        <LinkButton
          onClick={handleLaunchSearchResults}
          className={classes.backToSearchBtn}
        >
          Back to search results
        </LinkButton>
      </div>
      <div className={classes.heavyText}>
        {occurrencesInDocumentCount}{" "}
        {occurrencesInDocumentCount === 1 ? "match" : "matches"} for "
        {searchTerm}" in {presentationTitle}
      </div>

      <HeaderSearchModeNavigation
        occurrencesInDocumentCount={occurrencesInDocumentCount}
        focussedHighlightIndex={focussedHighlightIndex}
        handleSetFocussedHighlightIndex={handleSetFocussedHighlightIndex}
      />
    </div>
  );
};
