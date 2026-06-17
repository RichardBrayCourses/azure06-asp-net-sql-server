import { createContext, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { refreshConsoleViewModels, setConsoleDatabase } from "@/data/console";
import { ApiBackedAllChecksOutDatabase } from "@/data/apiRepository";
import { useAuth } from "./AuthContext";

const db = new ApiBackedAllChecksOutDatabase();
setConsoleDatabase(db);

type DomainDataContextValue = {
  db: ApiBackedAllChecksOutDatabase;
  refresh: () => Promise<void>;
  version: number;
  error: string | null;
  loading: boolean;
};

const DomainDataContext = createContext<DomainDataContextValue | null>(null);

export function useDomainData() {
  const value = useContext(DomainDataContext);
  if (!value) throw new Error("useDomainData must be used within <DomainDataProvider>");
  return value;
}

export default function DomainDataProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [version, setVersion] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      if (user.isLoggedIn) {
        await db.hydrateFromApi();
      }
      refreshConsoleViewModels();
      setVersion((current) => current + 1);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Domain data could not be loaded from the API.");
      refreshConsoleViewModels();
      setVersion((current) => current + 1);
    } finally {
      setLoading(false);
    }
  }, [user.isLoggedIn]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const value = useMemo(
    () => ({
      db,
      refresh,
      version,
      error,
      loading,
    }),
    [error, loading, refresh, version],
  );

  return (
    <DomainDataContext.Provider value={value}>
      {children}
    </DomainDataContext.Provider>
  );
}
