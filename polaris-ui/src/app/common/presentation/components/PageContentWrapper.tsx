export const PageContentWrapper: React.FC<{
  className?: string;
  children: React.ReactNode;
}> = ({ children, className }) => (
  <main
    className={`govuk-main-wrapper ${className}`}
    id="main-content"
    role="main"
  >
    {children}
  </main>
);
