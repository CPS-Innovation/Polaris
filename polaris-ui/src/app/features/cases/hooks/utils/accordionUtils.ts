export type AccordionSectionOpenState = {
  sectionsOpenStatus: { [key: string]: boolean };
  isAllOpen: boolean;
};

export const buildAccordionSectionOpenInitialState = (ids: string[]) =>
  ids.reduce(
    (accumulator, current) => {
      accumulator.sectionsOpenStatus[current] = false;
      return accumulator;
    },
    { sectionsOpenStatus: {}, isAllOpen: false } as AccordionSectionOpenState
  );
