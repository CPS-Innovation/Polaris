import classes from "./CommentsTimeline.module.scss";

type CommentsTimelineProps = {
  comments?: { username: string; date: string; description: string }[];
};

const deafultComments = [
  {
    username: "rrr",
    date: "13February2022",
    description: "abc",
  },
  {
    username: "rrr1",
    date: "13February2022",
    description: "abc1",
  },
  {
    username: "rrr1",
    date: "13February2022",
    description: "abc1",
  },
  {
    username: "rrr1",
    date: "13February2022",
    description: "abc1",
  },
];

export const CommentsTimeline: React.FC<CommentsTimelineProps> = ({
  comments = deafultComments,
}) => {
  return (
    <div className={classes.commentsTimeline}>
      {comments.map((comment) => (
        <div className={classes.commentWrapper}>
          <h4 className={classes.commentHead}>{comment.username}</h4>
          <div className={classes.commentBody}>
            <span>{comment.date}</span>
            <span>{comment.description}</span>
          </div>
        </div>
      ))}
    </div>
  );
};
