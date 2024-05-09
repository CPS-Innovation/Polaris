import React, { forwardRef } from "react";
import classes from "./LinkButton.module.scss";

type LinkButtonProps = {
  id?: string;
  children: React.ReactNode;
  className?: string;
  dataTestId?: string;
  disabled?: boolean;
  ariaLabel?: string;
  ariaExpanded?: boolean;
  type?: "button" | "submit" | "reset";
  onClick: () => void;
  onFocus?: () => void;
};

export const LinkButton = forwardRef<HTMLButtonElement | null, LinkButtonProps>(
  (
    {
      children,
      className,
      dataTestId,
      onClick,
      onFocus,
      id,
      ariaLabel,
      ariaExpanded,
      disabled = false,
      type,
    },
    ref
  ) => {
    const resolvedClassName = `${classes.linkButton} ${className}`;
    return (
      <button
        ref={ref}
        aria-label={ariaLabel}
        aria-expanded={ariaExpanded}
        disabled={disabled}
        id={id}
        className={resolvedClassName}
        onClick={onClick}
        data-testid={dataTestId}
        type={type}
        onFocus={onFocus}
      >
        {children}
      </button>
    );
  }
);
