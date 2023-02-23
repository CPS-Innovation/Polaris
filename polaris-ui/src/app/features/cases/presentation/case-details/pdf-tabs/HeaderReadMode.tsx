import { useEffect } from "react";
import { LinkButton } from "../../../../../common/presentation/components/LinkButton";
import { CaseDocumentViewModel } from "../../../domain/CaseDocumentViewModel";
import classes from "./HeaderReadMode.module.scss";

type Props = {
  caseDocumentViewModel: Extract<CaseDocumentViewModel, { mode: "read" }>;
  handleOpenPdfInNewTab: (pdfId: number) => void;
};

export const HeaderReadMode: React.FC<Props> = ({
  caseDocumentViewModel: { presentationFileName, sasUrl, documentId },
  handleOpenPdfInNewTab,
}) => {
  useEffect(() => {
    if (sasUrl) {
      window.open(sasUrl, "_blank");
    }
  }, [sasUrl]);

  return (
    <div className={classes.content}>
      <LinkButton
        dataTestId="btn-open-pdf"
        onClick={() => handleOpenPdfInNewTab(documentId)}
      >
        {presentationFileName} (opens in a new window)
      </LinkButton>
    </div>
  );
};
