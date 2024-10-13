import { Spinner } from "./index";
import { ReactComponent as WhiteTickIcon } from "../svgs/whiteTick.svg";
import classes from "./SaveBanner.module.scss";
type Props = {
  status: "saving" | "saved";
  savingText: string;
  savedText: string;
};

export const SaveBanner: React.FC<Props> = ({
  status,
  savingText,
  savedText,
}) => (
  <div className={classes.saveBanner}>
    {status === "saving" && (
      <div className={classes.savingBanner} data-testid="rl-saving-redactions">
        <div className={classes.spinnerWrapper}>
          <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
        </div>
        <h2 className={classes.bannerText}>{savingText}</h2>
      </div>
    )}

    {status === "saved" && (
      <div className={classes.savedBanner} data-testid="rl-saved-redactions">
        <WhiteTickIcon className={classes.whiteTickIcon} />
        <h2 className={classes.bannerText}>{savedText}</h2>
      </div>
    )}
  </div>
);
