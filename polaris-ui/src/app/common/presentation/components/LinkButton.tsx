import classes from "./LinkButton.module.scss";

type LinkButtonProps = {
  id?: string;
  className?: string;
  dataTestId?: string;
  disabled?: boolean;
  onClick: () => void;
};

export const LinkButton: React.FC<LinkButtonProps> = ({
  children,
  className,
  dataTestId,
  onClick,
  id,
  disabled = false,
}) => {
  const resolvedClassName = `${classes.linkButton} ${className}`;
  return (
    <button
      disabled={disabled}
      id={id}
      className={resolvedClassName}
      onClick={onClick}
      data-testid={dataTestId}
    >
      {children}
    </button>
  );
};
