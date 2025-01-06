import {
  ReactNode,
  createContext,
  useContext,
  useCallback,
  useMemo,
} from "react";
import { buildHeaders } from "../app/features/cases/api/auth/header-factory";

interface AuthHeaderProviderProps {
  children: ReactNode;
}

interface AuthHeaderContextType {
  buildHeaders: (correlationId?: string) => Promise<Record<string, string>>;
}

const AuthHeaderContext = createContext<AuthHeaderContextType | undefined>(
  undefined
);

export const AuthHeaderProvider: React.FC<AuthHeaderProviderProps> = (
  props
) => {
  const buildHeadersFn = useCallback(() => buildHeaders(), []);
  const initialValue = useMemo(
    () => ({
      buildHeaders: buildHeadersFn,
    }),
    [buildHeadersFn]
  );
  return (
    <AuthHeaderContext.Provider value={initialValue}>
      {props.children}
    </AuthHeaderContext.Provider>
  );
};

export const useAuthHeaderContext = () => {
  const context = useContext(AuthHeaderContext);
  if (!context) {
    throw new Error("header context should not be undefined");
  }
  return context;
};
