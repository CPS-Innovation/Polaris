import classes from "./NotesTimeline.module.scss";
import {
  formatDate,
  CommonDateTimeFormats,
} from "../../../../../common/utils/dates";
import { Note } from "../../../domain/gateway/NotesData";

type NotesTimelineProps = {
  notes: Note[];
};

export const NotesTimeline: React.FC<NotesTimelineProps> = ({ notes }) => {
  return (
    <ul className={classes.notesTimeline} data-testid="notes-list">
      {notes?.map((note) => (
        <li className={classes.noteWrapper} key={note.sortOrder}>
          <span className={classes.visuallyHidden}> Note added by</span>
          <span
            className={classes.noteHead}
            data-testid={`created-by-${note.sortOrder}`}
          >
            {note.createdByName}
          </span>
          <span className={classes.visuallyHidden}> added on</span>
          <span
            className={classes.noteDate}
            data-testid={`added-on-${note.sortOrder}`}
          >
            {formatDate(
              note.date,
              CommonDateTimeFormats.ShortDateFullTextMonth
            )}
          </span>
          <span className={classes.visuallyHidden}> content</span>
          <span
            className={classes.noteText}
            data-testid={`note-text-${note.sortOrder}`}
          >
            {note.text}
          </span>
        </li>
      ))}
    </ul>
  );
};
