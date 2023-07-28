import classes from "./LinkButton.module.scss";

type LinkButtonProps = {
  id?: string;
  className?: string;
  dataTestId?: string;
  onClick: () => void;
};

export const LinkButton: React.FC<LinkButtonProps> = ({
  children,
  className,
  dataTestId,
  onClick,
  id,
}) => {
  const resolvedClassName = `${classes.linkButton} ${className}`;
  return (
    <button
      id={id}
      className={resolvedClassName}
      onClick={onClick}
      data-testid={dataTestId}
    >
      {children}
    </button>
  );
};
