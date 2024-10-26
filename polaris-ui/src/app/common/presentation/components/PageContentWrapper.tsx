export const PageContentWrapper: React.FC<{ className?: string }> = ({
  children,
  className,
}) => (
  <main
    className={`govuk-main-wrapper ${className}`}
    id="main-content"
    role="main"
  >
    {children}
  </main>
);
