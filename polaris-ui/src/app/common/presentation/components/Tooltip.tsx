import React, { useState } from "react";
import classes from "./Tooltip.module.scss";

type TooltipProps = {
  text: string;
  children: React.ReactNode;
  className?: string;
  dataTestId?: string;
};

export const Tooltip: React.FC<TooltipProps> = ({
  text,
  children,
  className = "",
  dataTestId = "tooltip",
}) => {
  const [showTooltip, setShowTooltip] = useState(false);

  const handleMouseEnter = () => {
    setShowTooltip(true);
  };

  const handleMouseLeave = () => {
    setShowTooltip(false);
  };

  return (
    <div
      className={classes.tooltipContainer}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {children}
      {showTooltip && (
        <div
          className={`${classes.tooltip} ${classes.tooltipRight}`}
          data-testid={dataTestId}
        >
          {text}
        </div>
      )}
    </div>
  );
};

export default Tooltip;
