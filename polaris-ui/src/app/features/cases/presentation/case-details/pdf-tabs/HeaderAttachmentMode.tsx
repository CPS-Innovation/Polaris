import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import { ReactComponent as Attachment } from "../../../../../common/presentation/svgs/attachment.svg";
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
        {attachments.map((item) => (
          <li key={item.documentId} className={classes.tabListItem}>
            <Attachment className={classes.attachmentIcon} />
            <LinkButton
              onClick={() => {
                handleAttachmentClick(item.documentId);
              }}
            >
              {item.name}
              <span className={classes.separator}>,</span>
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
