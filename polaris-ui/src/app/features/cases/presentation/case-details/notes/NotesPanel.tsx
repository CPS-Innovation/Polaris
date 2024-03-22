import { useState } from "react";
import {
  Button,
  LinkButton,
  CharacterCount,
} from "../../../../../common/presentation/components";
import { CommentsTimeline } from "./CommentsTimeline";
import classes from "./NotesPanel.module.scss";

const NOTES_MAX_CHARACTERS = 500;
type NotesPanelProps = {};

export const NotesPanel: React.FC<NotesPanelProps> = () => {
  const [newNoteValue, setNewNoteValue] = useState("");

  return (
    <div className={classes.notesPanel}>
      <div className={classes.notesHeader}> Notes Header</div>
      <div className={classes.notesBody}>
        <h2> Add a note to the document</h2>

        <CharacterCount
          //   errorMessage={
          //     errorState.notes
          //       ? {
          //           children: `Supporting notes must be ${NOTES_MAX_CHARACTERS} characters or less`,
          //         }
          //       : undefined
          //   }
          value={newNoteValue}
          onChange={(event) => {
            setNewNoteValue(event.target.value);
          }}
          name="new-note"
          maxlength={NOTES_MAX_CHARACTERS}
          id="redaction-log-notes"
          data-testid="redaction-log-notes"
          label={{
            children: (
              <span className={classes.textAreaLabel}>
                Supporting notes{" "}
                <span className={classes.greyColor}>(optional)</span>
              </span>
            ),
          }}
        />

        <div className={classes.btnWrapper}>
          <Button
            disabled={!newNoteValue.length}
            type="submit"
            className={classes.saveBtn}
            data-testid="btn-save-redaction-log"
          >
            Add note
          </Button>

          <LinkButton
            className={classes.cancelBtn}
            onClick={() => {}}
            dataTestId="btn-redaction-log-cancel"
          >
            Cancel
          </LinkButton>
        </div>
      </div>
      <CommentsTimeline />
    </div>
  );
};
