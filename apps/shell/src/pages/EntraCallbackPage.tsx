import { useAuth } from "@/context/AuthContext";
import { useDomainData } from "@/context/DomainDataContext";
import { getDefaultConsolePath } from "@/data/console";
import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

export default function EntraCallbackPage() {
  const { completeEntraSignIn } = useAuth();
  const { db, refresh } = useDomainData();
  const navigate = useNavigate();
  const handled = useRef(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (handled.current) return;
    handled.current = true;

    completeEntraSignIn()
      .then(async ({ selection, identity }) => {
        try {
          await db.registerUserAccountWithEntra(selection.authenticatableUserId, identity.objectId);
        } catch {
          // First-time demo sign-in can reach this before the API user is linked.
        }
        await refresh();
        navigate(getDefaultConsolePath(selection.role), { replace: true });
      })
      .catch((caught: unknown) => {
        setError(caught instanceof Error ? caught.message : "Entra sign-in failed.");
      });
  }, [completeEntraSignIn, db, navigate, refresh]);

  return (
    <main className="min-h-screen bg-[#f8f8f8] px-4 py-10 text-[#0b0c0c] dark:bg-background dark:text-foreground sm:px-6">
      <div className="mx-auto max-w-xl">
        <h1 className="text-3xl font-bold">Completing sign in</h1>
        {error ? (
          <div className="mt-6 border-l-4 border-[#d4351c] bg-white p-4 text-sm dark:bg-card">
            <p className="font-bold">The Entra authentication could not be completed.</p>
            <p className="mt-2">{error}</p>
          </div>
        ) : (
          <p className="mt-4 text-sm text-[#505a5f] dark:text-muted-foreground">
            Checking the Entra response and opening your selected account context.
          </p>
        )}
      </div>
    </main>
  );
}
