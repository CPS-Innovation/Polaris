import { buildInitialState, AccordionReducerState, reducer } from "./reducer";

describe("accordion reducer", () => {
  describe("buildInitialState", () => {
    it("can build initial state", () => {
      const sectionIds = ["foo", "bar", "baz"];

      const initialState = buildInitialState(sectionIds);

      expect(initialState).toEqual({
        sections: { foo: false, bar: false, baz: false },
        isAllOpen: false,
      } as AccordionReducerState);
    });
  });

  describe("reducer", () => {
    describe("per-section open/close", () => {
      it("can open a section, leaving a mix of open and closed sections", () => {
        const state: AccordionReducerState = {
          sections: { foo: false, bar: false },
          isAllOpen: false,
        };

        const nextState = reducer(state, {
          type: "OPEN_CLOSE",
          payload: { id: "foo", open: true },
        });

        expect(nextState).toEqual({
          sections: { foo: true, bar: false },
          isAllOpen: false,
        } as AccordionReducerState);
      });
      it("can open a section, leaving all sections opened", () => {
        const state: AccordionReducerState = {
          sections: { foo: false, bar: true },
          isAllOpen: false,
        };

        const nextState = reducer(state, {
          type: "OPEN_CLOSE",
          payload: { id: "foo", open: true },
        });

        expect(nextState).toEqual({
          sections: { foo: true, bar: true },
          isAllOpen: true,
        } as AccordionReducerState);
      });
      it("can close a section", () => {
        const state: AccordionReducerState = {
          sections: { foo: true, bar: true },
          isAllOpen: true,
        };

        const nextState = reducer(state, {
          type: "OPEN_CLOSE",
          payload: { id: "foo", open: false },
        });

        expect(nextState).toEqual({
          sections: { foo: false, bar: true },
          isAllOpen: false,
        } as AccordionReducerState);
      });
    });
    describe("open/close all", () => {
      it("can open all sections", () => {
        const state: AccordionReducerState = {
          sections: { foo: false, bar: false },
          isAllOpen: false,
        };

        const nextState = reducer(state, {
          type: "OPEN_CLOSE_ALL",
          payload: true,
        });

        expect(nextState).toEqual({
          sections: { foo: true, bar: true },
          isAllOpen: true,
        } as AccordionReducerState);
      });
      it("can close all sections", () => {
        const state: AccordionReducerState = {
          sections: { foo: true, bar: true },
          isAllOpen: true,
        };

        const nextState = reducer(state, {
          type: "OPEN_CLOSE_ALL",
          payload: false,
        });

        expect(nextState).toEqual({
          sections: { foo: false, bar: false },
          isAllOpen: false,
        } as AccordionReducerState);
      });
    });
  });
});
