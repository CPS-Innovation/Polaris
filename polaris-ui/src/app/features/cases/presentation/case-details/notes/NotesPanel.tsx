import { useState } from "react";
import {
  Button,
  LinkButton,
  CharacterCount,
} from "../../../../../common/presentation/components";
import { CommentsTimeline } from "./CommentsTimeline";
import classes from "./NotesPanel.module.scss";
import { NotesData } from "../../../domain/gateway/NotesData";

const NOTES_MAX_CHARACTERS = 500;
type NotesPanelProps = {
  documentId: string;
  documentCategory: string;
  notesData: NotesData[];
  handleAddNote: (
    documentId: string,
    documentCategory: string,
    notesText: string
  ) => void;
  handleCloseNotes: () => void;
};

export const NotesPanel: React.FC<NotesPanelProps> = ({
  notesData,
  documentId,
  documentCategory,
  handleCloseNotes,
  handleAddNote,
}) => {
  const [newNoteValue, setNewNoteValue] = useState("");

  const handleAddBtnClick = () => {
    handleAddNote(documentId, documentCategory, newNoteValue);
  };

  return (
    <div className={classes.notesPanel}>
      <div className={classes.notesHeader}>
        {" "}
        <h3>Notes Header</h3>
      </div>
      <div className={classes.notesBody}>
        <div className={classes.notesTextArea}>
          <h4 className={classes.addNoteHeading}>Add a note to the document</h4>

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
            id="notes-textarea"
            data-testid="notes-textarea"
            label={{
              children: (
                <span className={classes.textAreaLabel}>Notes Text Area</span>
              ),
            }}
          />

          <div className={classes.btnWrapper}>
            <Button
              disabled={!newNoteValue.length}
              type="submit"
              className={classes.saveBtn}
              data-testid="btn-save-redaction-log"
              onClick={handleAddBtnClick}
            >
              Add note
            </Button>

            <LinkButton
              className={classes.cancelBtn}
              onClick={() => handleCloseNotes()}
              dataTestId="btn-redaction-log-cancel"
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      </div>
      <CommentsTimeline
        notes={
          notesData.find((note) => note.documentId === documentId)?.notes ?? []
        }
      />
    </div>
  );
};
