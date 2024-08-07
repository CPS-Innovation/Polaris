import { useState, useMemo, useEffect, useRef } from "react";
import {
  Button,
  LinkButton,
  CharacterCount,
  ErrorSummary,
  Modal,
} from "../../../../../common/presentation/components";
import { NotesTimeline } from "./NotesTimeline";
import classes from "./NotesPanel.module.scss";
import { NotesData } from "../../../domain/gateway/NotesData";
import { ReactComponent as CloseIcon } from "../../../../../common/presentation/svgs/closeIconBold.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { useLastFocus } from "../../../../../common/hooks/useLastFocus";

const NOTES_MAX_CHARACTERS = 500;
type NotesPanelProps = {
  documentName: string;
  documentId: string;
  documentCategory: string;
  notesData: NotesData[];
  activeDocumentId: string;
  handleAddNote: (documentId: string, notesText: string) => void;
  handleCloseNotes: () => void;
  handleGetNotes: (documentId: string) => void;
};

export const NotesPanel: React.FC<NotesPanelProps> = ({
  documentName,
  notesData,
  documentId,
  documentCategory,
  activeDocumentId,
  handleCloseNotes,
  handleAddNote,
  handleGetNotes,
}) => {
  useLastFocus(`#btn-notes-${documentId}`);
  const cancelBtnRef = useRef(null);
  const errorSummaryRef = useRef(null);
  const trackEvent = useAppInsightsTrackEvent();
  const [newNoteValue, setNewNoteValue] = useState("");
  const [oldNoteValue, setOldNoteValue] = useState("");
  const [notesError, setNotesError] = useState(false);
  const [showMismatchAlert, setShowMismatchAlert] = useState(false);

  const handleAddBtnClick = () => {
    if (newNoteValue.length > NOTES_MAX_CHARACTERS) {
      setNotesError(true);
      if (notesError && errorSummaryRef.current) {
        (errorSummaryRef?.current as HTMLButtonElement).focus();
      }
      return;
    }

    if (cancelBtnRef.current) {
      (cancelBtnRef.current as HTMLElement).focus();
    }
    if (notesError) {
      setNotesError(false);
    }
    if (activeDocumentId && activeDocumentId !== documentId) {
      setShowMismatchAlert(true);
      return;
    }
    saveNote();
  };

  const saveNote = () => {
    trackEvent("Add Note", {
      documentId: documentId,
      documentCategory: documentCategory,
    });
    setOldNoteValue(newNoteValue);
    setNewNoteValue("");
    handleAddNote(documentId, newNoteValue);
  };

  const handleMismatchWarningOk = () => {
    trackEvent("Notes Document Mismatch Ok", {
      documentId: documentId,
      documentCategory: documentCategory,
    });
    setShowMismatchAlert(false);
    saveNote();
  };

  const handleMismatchWarningCancel = (closeBtn = false) => {
    trackEvent("Notes Document Mismatch Cancel", {
      documentId: documentId,
      documentCategory: documentCategory,
      closeBtn: closeBtn,
    });
    setShowMismatchAlert(false);
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
    if (notesError && errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [notesError]);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (noteData?.addNoteStatus === "success") {
        handleCloseNotes();
      }
    }, 500);
    return () => {
      clearTimeout(timer);
    };
  }, [noteData?.addNoteStatus, handleCloseNotes]);

  useEffect(() => {
    handleGetNotes(documentId);
  }, []);

  const notesList = useMemo(() => {
    const notes = noteData?.notes ?? [];
    return [...notes].reverse();
  }, [noteData]);

  const addNoteSuccessLiveText = useMemo(() => {
    if (noteData?.addNoteStatus === "success") {
      return "Note added successfully";
    }
    return "";
  }, [noteData?.addNoteStatus]);

  const notesCountLiveText = useMemo(() => {
    if (noteData?.addNoteStatus === "initial" && notesList.length) {
      return notesList.length > 1
        ? ` there are ${notesList.length} notes available for this document`
        : "there is one note available for this document";
    }
  }, [notesList.length, noteData?.addNoteStatus]);

  return (
    <div className={classes.notesPanel} data-testid="notes-panel">
      <div className={classes.notesHeader}>
        {" "}
        <h3 className={classes.notesTitle}>
          {" "}
          Notes -{" "}
          <span className={classes.notesDocumentName}>{documentName}</span>
        </h3>
        <LinkButton
          dataTestId="btn-close-notes"
          type="button"
          className={classes.notesPanelCloseBtn}
          ariaLabel="close notes"
          onClick={() => handleCloseNotes()}
        >
          <CloseIcon height={"2.5rem"} width={"2.5rem"} />
        </LinkButton>
        <div
          role="status"
          aria-live="polite"
          className={classes.visuallyHidden}
        >
          {addNoteSuccessLiveText}
        </div>
        <div
          role="status"
          aria-live="polite"
          className={classes.visuallyHidden}
        >
          {notesCountLiveText}
        </div>
      </div>
      <div className={classes.notesBody}>
        {notesError && (
          <div
            ref={errorSummaryRef}
            tabIndex={-1}
            className={classes.errorSummaryWrapper}
          >
            <ErrorSummary
              data-testid={"notes-error-summary"}
              className={classes.errorSummary}
              errorList={[
                {
                  reactListKey: "1",
                  children: `Notes must be ${NOTES_MAX_CHARACTERS} characters or less`,
                  href: "#notes-textarea",
                  "data-testid": "notes-textarea-link",
                },
              ]}
            />
          </div>
        )}
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
            maxCharacters={NOTES_MAX_CHARACTERS}
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
              data-testid="btn-add-note"
              onClick={handleAddBtnClick}
            >
              Save and close
            </Button>

            <LinkButton
              ref={cancelBtnRef}
              className={classes.cancelBtn}
              onClick={() => handleCloseNotes()}
              dataTestId="btn-cancel-notes"
              ariaLabel="cancel notes"
              id="btn-cancel-notes"
            >
              Cancel
            </LinkButton>
          </div>
        </div>
      </div>
      <NotesTimeline notes={notesList} />

      {showMismatchAlert && (
        <Modal
          isVisible
          handleClose={() => {
            handleMismatchWarningCancel(true);
          }}
          type="alert"
          ariaLabel="Notes document mismatch warning modal"
          ariaDescription="Check note will be added to the correct document. The note will be added to a different document to the one currently
              being viewed."
        >
          <div className={classes.alertContent}>
            <h1 className="govuk-heading-l">
              Check note will be added to the correct document
            </h1>
            <p>
              The note will be added to a different document to the one
              currently being viewed.
            </p>
            <div className={classes.actionButtonsWrapper}>
              <Button
                onClick={handleMismatchWarningOk}
                data-testid="btn-mismatch-ok"
              >
                Ok
              </Button>
              <LinkButton
                onClick={() => handleMismatchWarningCancel()}
                dataTestId="btn-mismatch-cancel"
              >
                cancel
              </LinkButton>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
