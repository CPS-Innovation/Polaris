import { SearchPIIData } from "../../../../cases/domain/gateway/SearchPIIData";
import classes from "./HeaderSearchPIIMode.module.scss";

type Props = {
  searchPIIData?: SearchPIIData;
};

export const HeaderSearchPIIMode: React.FC<Props> = ({ searchPIIData }) => {
  return (
    <div className={classes.headerSearchPIIMode}>
      <h4 className={classes.title}>Potential redactions</h4>
      <span>
        The following terms are items that could potentially be redacted in this
        document:{" "}
      </span>
      <ul className={classes.pIITypesList}>
        <li className={classes.pIITypesListItem}>(25) Named individuals,</li>
        <li className={classes.pIITypesListItem}>(2) Email addresses,</li>
        <li className={classes.pIITypesListItem}>(2) Previous convictions,</li>
        <li className={classes.pIITypesListItem}>(1) Date of birth</li>
      </ul>
      <span className={classes.attachmentCountText}>{}</span>
    </div>
  );
};
