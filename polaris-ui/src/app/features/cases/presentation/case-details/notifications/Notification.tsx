import { useMemo } from "react";
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
  const notificationListItemLabel = useMemo(() => {
    switch (evt.reason) {
      case "New":
        return `new document ${evt.presentationTitle} under ${evt.narrative} is available`;
      case "New Version":
        return `new version for the document ${evt.presentationTitle} under ${evt.narrative} is available`;
      case "Updated":
        return `updated document ${evt.presentationTitle} under ${evt.narrative}`;
      case "Reclassified":
        return `reclassified document ${evt.presentationTitle} under ${evt.narrative}`;
      case "Discarded":
        return `discarded document ${evt.presentationTitle}`;
    }
  }, [evt]);
  return (
    <li
      aria-label={notificationListItemLabel}
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
              ariaLabel={`open ${evt.presentationTitle}`}
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
        </div>
        <span className={`${classes.dateTime} govuk-body-s`}>
          <Time dateTime={evt.dateTime} />
        </span>
      </div>
      <div>
        <Button
          aria-label={"clear notification"}
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
