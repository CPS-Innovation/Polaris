import { Button } from "../../../../../common/presentation/components";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { IPdfHighlight } from "../../../domain/IPdfHighlight";
import classes from "./Footer.module.scss";

type Props = {
  tabIndex: number;
  redactionHighlights: IPdfHighlight[];
  handleRemoveAllRedactions: () => void;
  handleSavedRedactions: () => void;
};

export const Footer: React.FC<Props> = ({
  tabIndex,
  redactionHighlights,
  handleRemoveAllRedactions,
  handleSavedRedactions,
}) => {
  return (
    <div className={classes.footer}>
      <div className={classes.removeButton}>
        <LinkButton
          onClick={handleRemoveAllRedactions}
          dataTestId="link-removeAll"
        >
          Remove all redactions
        </LinkButton>
      </div>

      <div className={classes.summary}>
        {redactionHighlights.length === 1 ? (
          <>There is 1 redaction</>
        ) : (
          <>There are {redactionHighlights.length} redactions</>
        )}
      </div>

      <Button
        className={classes.saveButton}
        onClick={handleSavedRedactions}
        data-testid={`btn-save-redaction-${tabIndex}`}
      >
        Save all redactions
      </Button>
    </div>
  );
};
