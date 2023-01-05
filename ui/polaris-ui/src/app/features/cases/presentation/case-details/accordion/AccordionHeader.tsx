import classes from "./Accordion.module.scss";

type Props = {
  isAllOpen: boolean;
  handleToggleOpenAll: () => void;
};

export const AccordionHeader: React.FC<Props> = ({
  isAllOpen,
  handleToggleOpenAll: handleToggleAllOpen,
}) => {
  return (
    <div className={`${classes["accordion-controls"]}`}>
      <button
        className={`${classes["accordion-expand-all"]}`}
        aria-expanded={isAllOpen}
        onClick={handleToggleAllOpen}
        data-testid="btn-accordion-open-close-all"
      >
        {isAllOpen ? "Close all folders" : "Open all folders"}
      </button>
    </div>
  );
};
