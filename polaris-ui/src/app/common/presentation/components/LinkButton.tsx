import classes from "./LinkButton.module.scss";

type LinkButtonProps = {
  className?: string;
  dataTestId?: string;
  onClick: () => void;
};

export const LinkButton: React.FC<LinkButtonProps> = ({
  children,
  className,
  dataTestId,
  onClick,
}) => {
  const resolvedClassName = `${classes.linkButton} ${className}`;
  return (
    <button
      className={resolvedClassName}
      onClick={onClick}
      data-testid={dataTestId}
    >
      {children}
    </button>
  );
};
