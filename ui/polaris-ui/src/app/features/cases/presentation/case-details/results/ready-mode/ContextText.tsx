import { MappedTextSearchResult } from "../../../../domain/MappedTextSearchResult";

type Props = {
  contextTextChunks: MappedTextSearchResult["documentResults"][number]["occurrences"][number]["contextTextChunks"];
};

export const ContextText: React.FC<Props> = ({ contextTextChunks }) => {
  return (
    <div className="govuk-details__text">
      {contextTextChunks.map((chunk, index) => (
        <span key={index}>
          {chunk.isHighlighted ? (
            <>
              <b>{chunk.text}</b>{" "}
            </>
          ) : (
            <>
              <span>{chunk.text}</span>{" "}
            </>
          )}
        </span>
      ))}
    </div>
  );
};
