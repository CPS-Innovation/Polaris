import React, { forwardRef } from "react";
import classes from "./LinkButton.module.scss";

type LinkButtonProps = {
  id?: string;
  children: React.ReactNode;
  className?: string;
  dataTestId?: string;
  disabled?: boolean;
  onClick: () => void;
};

export const LinkButton = forwardRef<HTMLButtonElement | null, LinkButtonProps>(
  ({ children, className, dataTestId, onClick, id, disabled = false }, ref) => {
    const resolvedClassName = `${classes.linkButton} ${className}`;
    return (
      <button
        ref={ref}
        disabled={disabled}
        id={id}
        className={resolvedClassName}
        onClick={onClick}
        data-testid={dataTestId}
      >
        {children}
      </button>
    );
  }
);
