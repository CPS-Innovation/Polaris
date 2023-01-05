import classes from "./Spinner.module.scss";

type Props = {
  diameterPx: number;
};

export const Spinner: React.FC<Props> = ({ diameterPx }) => (
  <div
    className={classes.spinner}
    style={{ height: diameterPx, width: diameterPx }}
    aria-label="Pdf loading, please wait..."
  ></div>
);
