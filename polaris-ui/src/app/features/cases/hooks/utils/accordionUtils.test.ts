import {
  buildAccordionSectionOpenInitialState,
  AccordionSectionOpenState,
} from "./accordionUtils";

describe("buildAccordionSectionOpenInitialState", () => {
  it("can build initial state of accordion section open", () => {
    const sectionIds = ["foo", "bar", "baz"];

    const initialState = buildAccordionSectionOpenInitialState(sectionIds);

    expect(initialState).toEqual({
      sectionsOpenStatus: { foo: false, bar: false, baz: false },
      isAllOpen: false,
    } as AccordionSectionOpenState);
  });
});
