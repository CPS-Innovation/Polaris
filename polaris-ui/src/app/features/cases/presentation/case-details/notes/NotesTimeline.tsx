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
    <div className={classes.notesTimeline}>
      {notes?.map((note) => (
        <div className={classes.noteWrapper}>
          <h4 className={classes.noteHead}>{note.createdByName}</h4>
          <div className={classes.noteBody}>
            <span className={classes.noteDate}>
              {formatDate(
                note.date,
                CommonDateTimeFormats.ShortDateFullTextMonth
              )}
            </span>
            <span>{note.text}</span>
          </div>
        </div>
      ))}
    </div>
  );
};
