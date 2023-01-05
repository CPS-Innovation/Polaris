import { Details } from "../../../../../../common/presentation/components";
import { CombinedState } from "../../../../domain/CombinedState";
import classes from "./MissingDocs.module.scss";

type Props = {
  missingDocs: CombinedState["searchState"]["missingDocs"];
};

const NUMBER_DOCS_TO_EXPLICITYLY_SHOW = 4;

export const MissingDocs: React.FC<Props> = ({ missingDocs }) => {
  const firstFourDocs = missingDocs.slice(0, NUMBER_DOCS_TO_EXPLICITYLY_SHOW);
  const remainingDocs = missingDocs.slice(NUMBER_DOCS_TO_EXPLICITYLY_SHOW);

  return (
    <>
      <div>
        <p>Technical problems stopped us from searching these documents:</p>
        <ul className={classes.docList}>
          {firstFourDocs.map(({ documentId, fileName }) => (
            <li key={documentId} data-testid={`txt-missing-doc-${documentId}`}>
              {fileName}
            </li>
          ))}
        </ul>

        {!!remainingDocs.length && (
          <Details
            data-testid="details-expand-missing-docs"
            isDefaultLeftBorderHidden
            summaryChildren={`View ${remainingDocs.length} more`}
            children={
              <ul className={classes.docList}>
                {remainingDocs.map(({ documentId, fileName }) => (
                  <li
                    key={documentId}
                    data-testid={`txt-missing-doc-${documentId}`}
                  >
                    {fileName}
                  </li>
                ))}
              </ul>
            }
          />
        )}
      </div>
    </>
  );
};
