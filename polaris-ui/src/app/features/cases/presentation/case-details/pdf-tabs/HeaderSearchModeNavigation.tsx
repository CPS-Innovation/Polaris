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
      {focussedHighlightIndex > 0 && (
        <LinkButton
          className={classes.previousBtn}
          dataTestId="btn-focus-highlight-previous"
          onClick={() =>
            handleSetFocussedHighlightIndex(focussedHighlightIndex - 1)
          }
        >
          Previous
        </LinkButton>
      )}
      <span
        className={classes.numbers}
        data-testid="txt-focus-highlight-numbers"
      >
        {oneBasedFocussedHighlightIndex}/{occurrencesInDocumentCount}
      </span>

      {focussedHighlightIndex < occurrencesInDocumentCount - 1 && (
        <LinkButton
          className={classes.nextBtn}
          dataTestId="btn-focus-highlight-next"
          onClick={() =>
            handleSetFocussedHighlightIndex(focussedHighlightIndex + 1)
          }
        >
          Next
        </LinkButton>
      )}
    </div>
  );
};
