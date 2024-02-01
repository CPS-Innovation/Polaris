import React, { useState } from "react";
import classes from "./Tooltip.module.scss";

type TooltipProps = {
  text: string;
  children: React.ReactNode;
  position?: "top" | "bottom" | "left" | "right";
  className?: string;
};

export const Tooltip: React.FC<TooltipProps> = ({
  text,
  children,
  position = "bottom",
  className = "",
}) => {
  const [showTooltip, setShowTooltip] = useState(false);

  const handleMouseEnter = () => {
    setShowTooltip(true);
  };

  const handleMouseLeave = () => {
    setShowTooltip(false);
  };

  const getPositionClass = () => {
    switch (position) {
      case "right":
        return classes.tooltipRight;
      case "left":
        return classes.tooltipRight;
      case "top":
        return classes.tooltipBottom;
      default:
        return classes.tooltipBottom;
    }
  };

  return (
    <div
      className={classes.tooltipContainer}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {children}
      {showTooltip && (
        <div className={`${classes.tooltip} ${getPositionClass()}`}>{text}</div>
      )}
    </div>
  );
};

export default Tooltip;
