import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import classes from "./HeaderSearchModeNavigation.module.scss";

type Props = {
  occurrencesInDocumentCount: number;
  focussedHighlightIndex: number;
  handleSetFocussedHighlightIndex: (nextIndex: number) => void;
};

export const HeaderSearchModeNavigation: React.FC<Props> = ({
  occurrencesInDocumentCount,
  focussedHighlightIndex,
  handleSetFocussedHighlightIndex,
}) => {
  const oneBasedFocussedHighlightIndex = focussedHighlightIndex + 1;

  return (
    <div className={classes.container}>
      <span className={classes.previous}>
        {focussedHighlightIndex > 0 ? (
          <LinkButton
            data-testid="btn-focus-highlight-previous"
            onClick={() =>
              handleSetFocussedHighlightIndex(focussedHighlightIndex - 1)
            }
          >
            Previous
          </LinkButton>
        ) : (
          <span data-testid="txt-focus-highlight-previous">Previous</span>
        )}
      </span>
      <span
        className={classes.numbers}
        data-testid="txt-focus-highlight-numbers"
      >
        {oneBasedFocussedHighlightIndex}/{occurrencesInDocumentCount}
      </span>
      <span className={classes.next}>
        {focussedHighlightIndex < occurrencesInDocumentCount - 1 ? (
          <LinkButton
            data-testid="btn-focus-highlight-next"
            onClick={() =>
              handleSetFocussedHighlightIndex(focussedHighlightIndex + 1)
            }
          >
            Next
          </LinkButton>
        ) : (
          <span data-testid="txt-focus-highlight-next">Next</span>
        )}
      </span>
    </div>
  );
};
