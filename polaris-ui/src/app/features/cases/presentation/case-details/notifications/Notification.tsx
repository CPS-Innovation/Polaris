import {
  Button,
  LinkButton,
  Tag,
} from "../../../../../common/presentation/components";
import {
  NotificationEvent,
  NotificationReasonMap,
} from "../../../domain/NotificationState";
import { useCaseDetailsState } from "../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./notifications.module.scss";
import { Time } from "./Time";

export const Notification: React.FC<{
  evt: NotificationEvent;
  handleOpenPdf: ReturnType<typeof useCaseDetailsState>["handleOpenPdf"];
  handleClearNotification: (id: number) => void;
}> = ({ evt, handleOpenPdf, handleClearNotification }) => {
  return (
    <li
      key={evt.id}
      className={evt.status === "Live" ? classes.live : classes.read}
    >
      <div>
        <div>
          <Tag
            gdsTagColour={NotificationReasonMap[evt.reason]}
            className={classes.tag}
          >
            {evt.reason}
          </Tag>
        </div>
        <div>
          {evt.reason === "Discarded" ? (
            evt.presentationTitle
          ) : (
            <LinkButton
              className={classes.docLink}
              onClick={() =>
                handleOpenPdf({
                  documentId: evt.documentId,
                  mode: "read",
                })
              }
            >
              {evt.presentationTitle}
            </LinkButton>
          )}
        </div>
        <div className={`govuk-body-s ${classes.narrative}`}>
          <span>{evt.narrative}</span>
          {evt.reasonToIgnore ? (
            <span className={classes.reasonToIgnore}>{evt.reasonToIgnore}</span>
          ) : undefined}
        </div>
        <span className={`${classes.dateTime} govuk-body-s`}>
          <Time dateTime={evt.dateTime} />
        </span>
      </div>
      <div>
        <Button
          data-prevent-global-close
          onClick={() => handleClearNotification(evt.id)}
          className={`${classes.clear} govuk-button--secondary`}
        >
          Clear
        </Button>
      </div>
    </li>
  );
};
