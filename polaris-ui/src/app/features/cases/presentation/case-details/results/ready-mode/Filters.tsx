import { Label } from "../../../../../../common/presentation/components";
import {
  Checkboxes,
  CheckboxesProps,
} from "../../../../../../common/presentation/components/Checkboxes";
import { CombinedState } from "../../../../domain/CombinedState";
import { FilterOption } from "../../../../domain/FilterOption";
import { CaseDetailsState } from "../../../../hooks/use-case-details-state/useCaseDetailsState";
import classes from "./Filters.module.scss";

type Props = {
  filterOptions: CombinedState["searchState"]["filterOptions"];
  handleUpdateFilter: CaseDetailsState["handleUpdateFilter"];
};

const toItemArray = (obj: { [key: string]: FilterOption }) =>
  Object.entries(obj).map(([value, { label, count, isSelected }]) => ({
    id: value,
    value,
    children: `${label} (${count})`,
    checked: isSelected,
  })) as CheckboxesProps["items"];

export const Filters: React.FC<Props> = ({
  filterOptions: { docType, category },
  handleUpdateFilter,
}) => {
  const docTypeItems = toItemArray(docType);
  const categoryItems = toItemArray(category);
  const areResultsPresent = !!docTypeItems.length;

  return (
    <>
      <div className={`${classes.filterWrapper}`}>
        <h3
          data-testid="txt-filter-heading"
          className={`govuk-heading-m ${classes.heading}`}
        >
          Filter
        </h3>
      </div>

      {areResultsPresent && (
        <>
          <Label className="govuk-label--s" htmlFor="docType">
            Document type
          </Label>
          <Checkboxes
            data-testid="checkboxes-doc-type"
            name="docType"
            items={docTypeItems}
            className="govuk-checkboxes--small"
            onChange={(ev) => {
              handleUpdateFilter({
                filter: "docType",
                id: ev.currentTarget.id,
                isSelected: ev.target.checked,
              });
            }}
          />

          <Label className="govuk-label--s" htmlFor="category">
            Category
          </Label>
          <Checkboxes
            data-testid="checkboxes-category"
            name="category"
            items={categoryItems}
            className="govuk-checkboxes--small"
            onChange={(ev) => {
              handleUpdateFilter({
                filter: "category",
                id: ev.currentTarget.id,
                isSelected: ev.target.checked,
              });
            }}
          />
        </>
      )}
    </>
  );
};
