import { useState, useMemo, useEffect } from "react";
import {
  Button,
  LinkButton,
  CharacterCount,
} from "../../../../../common/presentation/components";
import { NotesTimeline } from "./NotesTimeline";
import classes from "./NotesPanel.module.scss";
import { NotesData } from "../../../domain/gateway/NotesData";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";

const NOTES_MAX_CHARACTERS = 500;
type NotesPanelProps = {
  documentName: string;
  documentId: string;
  documentCategory: string;
  notesData: NotesData[];
  handleAddNote: (
    documentId: string,
    documentCategory: string,
    notesText: string
  ) => void;
  handleCloseNotes: () => void;
  handleGetNotes: (documentId: string, documentCategory: string) => void;
};

export const NotesPanel: React.FC<NotesPanelProps> = ({
  documentName,
  notesData,
  documentId,
  documentCategory,
  handleCloseNotes,
  handleAddNote,
  handleGetNotes,
}) => {
  const trackEvent = useAppInsightsTrackEvent();
  const [newNoteValue, setNewNoteValue] = useState("");
  const [oldNoteValue, setOldNoteValue] = useState("");
  const [notesError, setNotesError] = useState(false);

  const handleAddBtnClick = () => {
    if (newNoteValue.length > NOTES_MAX_CHARACTERS) {
      setNotesError(true);
      return;
    }
    if (notesError) {
      setNotesError(false);
    }
    trackEvent("Add Note", {
      documentId: documentId,
    });
    setOldNoteValue(newNoteValue);
    setNewNoteValue("");
    handleAddNote(documentId, documentCategory, newNoteValue);
  };

  const noteData = useMemo(() => {
    return notesData.find((note) => note.documentId === documentId);
  }, [notesData, documentId]);

  useEffect(() => {
    if (noteData?.addNoteStatus === "failure") {
      setNewNoteValue(oldNoteValue);
    }
  }, [noteData?.addNoteStatus]);

  useEffect(() => {
    if (noteData?.addNoteStatus === "success") {
      handleGetNotes(documentId, documentCategory);
    }
  }, [noteData?.addNoteStatus, documentId, documentCategory, handleGetNotes]);

  useEffect(() => {
    handleGetNotes(documentId, documentCategory);
  }, []);

  const notesList = useMemo(() => {
    const notes = noteData?.notes ?? [];
    return [...notes].reverse();
  }, [noteData]);

  return (
    <div className={classes.notesPanel}>
      <div className={classes.notesHeader}>
        {" "}
        <h3 className={classes.notesTitle}>
          {" "}
          Notes -{" "}
          <span className={classes.notesDocumentName}>{documentName}</span>
        </h3>
      </div>
      <div className={classes.notesBody}>
        <div className={classes.notesTextArea}>
          <CharacterCount
            errorMessage={
              notesError
                ? {
                    children: `Notes must be ${NOTES_MAX_CHARACTERS} characters or less`,
                  }
                : undefined
            }
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
                <span className={classes.addNoteHeading}>
                  Add a note to the document
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
      <NotesTimeline notes={notesList} />
    </div>
  );
};