import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { ReactComponent as AttachmentIcon } from "../../../../../common/presentation/svgs/attachment.svg";
import { LinkButton } from "../../../../../common/presentation/components";
import classes from "./HeaderAttachmentMode.module.scss";

type Props = {
  caseDocumentViewModel: CaseDocumentViewModel;
  handleOpenPdf: (caseDocument: {
    documentId: string;
    mode: "read" | "search";
  }) => void;
};

export const HeaderAttachmentMode: React.FC<Props> = ({
  caseDocumentViewModel: { attachments },
  handleOpenPdf,
}) => {
  const handleAttachmentClick = (documentId: string) => {
    handleOpenPdf({ documentId, mode: "read" });
  };
  const getAttachmentText = () => {
    if (attachments.length === 1) return "1 attachment:";
    return `${attachments.length} attachments:`;
  };

  const renderAttachments = () => {
    return (
      <ul className={classes.attachmentsList}>
        {attachments.map((item, index) => (
          <li key={item.documentId} className={classes.tabListItem}>
            <AttachmentIcon className={classes.attachmentIcon} />
            <LinkButton
              dataTestId={`doc-attach-btn-${item.documentId}`}
              className={classes.documentBtn}
              onClick={() => {
                handleAttachmentClick(item.documentId);
              }}
            >
              {item.name}
              {index < attachments.length - 1 && (
                <span className={classes.separator}>,</span>
              )}
            </LinkButton>
          </li>
        ))}
      </ul>
    );
  };
  return (
    <div className={classes.content}>
      <span className={classes.attachmentCountText}>{getAttachmentText()}</span>

      <div>{renderAttachments()}</div>
    </div>
  );
};
