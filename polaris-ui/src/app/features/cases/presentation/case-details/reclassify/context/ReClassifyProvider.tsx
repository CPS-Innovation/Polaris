import { ReactNode, useReducer, createContext, useContext } from "react";
import {
  reClassifyReducer,
  reclassifyInitialState,
  ReclassifyActions,
  ReclassifyState,
} from "../reducer/reClassifyReducer";

interface ReClassifyProviderProps {
  children: ReactNode;
}

interface ReClassifyContextProps {
  state: ReclassifyState;
  dispatch: React.Dispatch<ReclassifyActions>;
}

const ReClassifyContext = createContext<ReClassifyContextProps | undefined>(
  undefined
);

export const ReClassifyProvider: React.FC<ReClassifyProviderProps> = (
  props
) => {
  const [state, dispatch] = useReducer(
    reClassifyReducer,
    reclassifyInitialState
  );

  return (
    <ReClassifyContext.Provider value={{ state, dispatch }}>
      {props.children}
    </ReClassifyContext.Provider>
  );
};

export const useReClassifyContext = () => {
  return useContext(ReClassifyContext);
};
