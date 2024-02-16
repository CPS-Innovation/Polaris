import React, { useState } from "react";
import classes from "./Tooltip.module.scss";

type TooltipProps = {
  text: string;
  children: React.ReactNode;
  position?: "top" | "bottom" | "left" | "right";
  className?: string;
  dataTestId?: string;
};

export const Tooltip: React.FC<TooltipProps> = ({
  text,
  children,
  className,
  position = "bottom",
  dataTestId = "tooltip",
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
        return classes.tooltipLeft;
      case "top":
        return classes.tooltipTop;
      default:
        return classes.tooltipBottom;
    }
  };

  return (
    <div
      className={`${classes.tooltipContainer} ${className ? className : ""}`}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {children}
      {showTooltip && (
        <div
          className={`${classes.tooltip} ${getPositionClass()}`}
          data-testid={dataTestId}
        >
          {text}
        </div>
      )}
    </div>
  );
};

export default Tooltip;
