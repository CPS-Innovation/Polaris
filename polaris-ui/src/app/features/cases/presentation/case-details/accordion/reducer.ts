export type AccordionReducerState = {
  sections: { [key: string]: boolean };
  isAllOpen: boolean;
};

export const reducer = (
  state: AccordionReducerState,
  action:
    | { type: "OPEN_CLOSE"; payload: { id: string; open: boolean } }
    | { type: "OPEN_CLOSE_ALL"; payload: boolean }
): AccordionReducerState => {
  switch (action.type) {
    case "OPEN_CLOSE": {
      const nextSections = {
        ...state.sections,
        [action.payload.id]: action.payload.open,
      };
      return {
        sections: nextSections,
        isAllOpen: Object.values(nextSections).every((value) => value === true),
      };
    }
    case "OPEN_CLOSE_ALL": {
      const nextSections = Object.keys(state.sections).reduce(
        (accumulator, current) => {
          accumulator[current] = action.payload;
          return accumulator;
        },
        {} as AccordionReducerState["sections"]
      );
      return {
        sections: nextSections,
        isAllOpen: action.payload,
      };
    }
  }
};

export const buildInitialState = (ids: string[]) =>
  ids.reduce(
    (accumulator, current) => {
      accumulator.sections[current] = false;
      return accumulator;
    },
    { sections: {}, isAllOpen: false } as AccordionReducerState
  );
