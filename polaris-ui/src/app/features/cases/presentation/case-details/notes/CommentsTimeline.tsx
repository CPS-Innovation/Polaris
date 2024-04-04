import classes from "./CommentsTimeline.module.scss";
import {
  formatDate,
  CommonDateTimeFormats,
} from "../../../../../common/utils/dates";
import { Note } from "../../../domain/gateway/NotesData";

type CommentsTimelineProps = {
  notes: Note[];
};

// const deafultComments = [
//   {
//     username: "rrr",
//     date: "13February2022",
//     description: "abc",
//   },
//   {
//     username: "rrr1",
//     date: "13February2022",
//     description: "abc1",
//   },
//   {
//     username: "rrr1",
//     date: "13February2022",
//     description: "abc1",
//   },
//   {
//     username: "rrr1",
//     date: "13February2022",
//     description: "abc1",
//   },
// ];

export const CommentsTimeline: React.FC<CommentsTimelineProps> = ({
  notes,
}) => {
  // const { notes } = notesData;
  return (
    <div className={classes.commentsTimeline}>
      {notes?.map((note) => (
        <div className={classes.commentWrapper}>
          <h4 className={classes.commentHead}>{note.createdByName}</h4>
          <div className={classes.commentBody}>
            <span>
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
