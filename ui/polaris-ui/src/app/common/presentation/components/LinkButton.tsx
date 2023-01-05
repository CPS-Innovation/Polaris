import classes from "./LinkButton.module.scss";

type AnchorProps = React.DetailedHTMLProps<
  React.AnchorHTMLAttributes<HTMLAnchorElement>,
  HTMLAnchorElement
>;

export const LinkButton: React.FC<AnchorProps> = ({
  children,
  className,
  ...props
}) => {
  const resolvedClassName = `${classes.linkButton} ${className}`;
  return (
    <a className={resolvedClassName} {...props}>
      {children}
    </a>
  );
};
